using System;
using System.Collections.Generic;
using System.Threading;
using WebCore.Collections;
using WebCore.Logging;
using WebCore.Platform;
using WebCore.Platform.Posix;
using WebCore.Utils;

namespace WebCore.LowMemory
{
    public interface ILowMemoryHandler
    {
        void LowMemory();
        void LowMemoryOver();
    }

    public class LowMemoryNotification
    {
        private const string NotificationThreadName = "Low memory notification thread";
        private readonly Logger _logger;
        private readonly ConcurrentSet<WeakReference<ILowMemoryHandler>> _lowMemoryHandlers = new ConcurrentSet<WeakReference<ILowMemoryHandler>>();
        public enum LowMemReason
        {
            None = 0,
            LowMemOnTimeoutChk,
            BackToNormal,
            BackToNormalSimulation,
            LowMemStateSimulation,
            BackToNormalHandler,
            LowMemHandler
        }

        public class LowMemEventDetails
        {
            public LowMemReason Reason;
            public long FreeMem;
            public DateTime Time;
            public long CurrentCommitCharge { get; set; }
            public long PhysicalMem { get; set; }
            public long TotalUnmanaged { get; set; }
            public long LowMemThreshold { get; set; }
        }

        public LowMemEventDetails[] LowMemEventDetailsStack = new LowMemEventDetails[256];
        private int _lowMemEventDetailsIndex;
        private int _clearInactiveHandlersCounter;

        private void RunLowMemoryHandlers(bool isLowMemory)
        {
            try
            {
                foreach (var lowMemoryHandler in _lowMemoryHandlers)
                {
                    if (lowMemoryHandler.TryGetTarget(out var handler))
                    {
                        try
                        {
                            if (isLowMemory)
                                handler.LowMemory();
                            else
                                handler.LowMemoryOver();
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                if (_logger.IsInfoEnabled)
                                    _logger.Info("Failure to process low memory notification (low memory handler - " + handler + ")", e);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                    else
                    {
                        // make sure that we aren't allocating here, we reserve 128 items
                        // and worst case we'll get it in the next run
                        if (_inactiveHandlers.Count < _inactiveHandlers.Capacity)
                            _inactiveHandlers.Add(lowMemoryHandler);
                    }
                }
                foreach (var x in _inactiveHandlers)
                {
                    _lowMemoryHandlers.TryRemove(x);
                }
            }
            finally
            {
                _inactiveHandlers.Clear();
            }
        }

        private void ClearInactiveHandlers()
        {
            var inactiveHandlers = new List<WeakReference<ILowMemoryHandler>>();

            foreach (var lowMemoryHandler in _lowMemoryHandlers)
            {
                if (lowMemoryHandler.TryGetTarget(out _) == false)
                    inactiveHandlers.Add(lowMemoryHandler);
            }

            inactiveHandlers.ForEach(x => _lowMemoryHandlers.TryRemove(x));
        }

        public void RegisterLowMemoryHandler(ILowMemoryHandler handler)
        {
            _lowMemoryHandlers.Add(new WeakReference<ILowMemoryHandler>(handler));
        }

        public static readonly LowMemoryNotification Instance = new LowMemoryNotification(
            new Size(1024 * 1024 * 256, SizeUnit.Bytes),
            0.05f);

        public bool LowMemoryState { get; set; }

        public static void Initialize(Size lowMemoryThreshold, float commitChargeThreshold, CancellationToken shutdownNotification)
        {
            Instance._lowMemoryThreshold = lowMemoryThreshold;
            Instance._commitChargeThreshold = commitChargeThreshold;

            shutdownNotification.Register(() => Instance._shutdownRequested.Set());
        }

        private Size _lowMemoryThreshold;
        private float _commitChargeThreshold;
        private readonly ManualResetEvent _simulatedLowMemory = new ManualResetEvent(false);
        private readonly ManualResetEvent _shutdownRequested = new ManualResetEvent(false);
        private readonly List<WeakReference<ILowMemoryHandler>> _inactiveHandlers = new List<WeakReference<ILowMemoryHandler>>(128);

        public LowMemoryNotification(Size lowMemoryThreshold, float commitChargeThreshold)
        {
            _logger = LoggingSource.Instance.GetLogger<LowMemoryNotification>("Server");

            _lowMemoryThreshold = lowMemoryThreshold;
            _commitChargeThreshold = commitChargeThreshold;

            var thread = new Thread(MonitorMemoryUsage)
            {
                IsBackground = true,
                Name = NotificationThreadName
            };

            thread.Start();
        }

        public static Size GetCurrentProcessMemoryMappedShared()
        {
            // because we are usually using memory mapped files, we don't want
            // to account for memory that was loaded into our own working set
            // but that the OS can discard with no cost (because it can load
            // the data from disk without needing to write it)

            var stats = MemoryInformation.MemoryStats();

            var sharedMemory = stats.WorkingSet - stats.TotalUnmanagedAllocations - stats.ManagedMemory - stats.MappedTemp;

            // if this is negative, we'll just ignore this
            var mappedShared = new Size(Math.Max(0, sharedMemory), SizeUnit.Bytes);
            return mappedShared;
        }

        private void MonitorMemoryUsage()
        {
            SmapsReader smapsReader = OS.IsLinux ? new SmapsReader(new[] {new byte[SmapsReader.BufferSize], new byte[SmapsReader.BufferSize]}) : null;
            NativeMemory.EnsureRegistered();
            var memoryAvailableHandles = new WaitHandle[] { _simulatedLowMemory, _shutdownRequested };
            var timeout = 5 * 1000;
            try
            {
                while (true)
                {
                    try
                    {
                        var result = WaitHandle.WaitAny(memoryAvailableHandles, timeout);
                        switch (result)
                        {
                            case WaitHandle.WaitTimeout:
                                timeout = CheckMemoryStatus(smapsReader);
                                break;
                            case 0:
                                SimulateLowMemory();
                                break;
                            case 1: // shutdown requested
                                return;
                            default:
                                return;
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        try
                        {
                            if (_logger.IsInfoEnabled)
                                _logger.Info("Out of memory error in the low memory notification thread, will wait 5 seconds before trying to check memory status again. The system is likely running out of memory");
                        }
                        catch
                        {
                            // can't even log, nothing we can do here
                        }

                        if (_shutdownRequested.WaitOne(5000))
                            return; // shutdown requested
                    }
                }
            }
            catch (Exception e)
            {
                if (_logger.IsOperationsEnabled)
                {
                    _logger.Operations("Catastrophic failure in low memory notification", e);
                }
            }
        }

        private void SimulateLowMemory()
        {
            _simulatedLowMemory.Reset();
            LowMemoryState = !LowMemoryState;
            var memInfoForLog = MemoryInformation.GetMemoryInfo();
            var availableMemForLog = memInfoForLog.AvailableMemory.GetValue(SizeUnit.Bytes);
            AddLowMemEvent(LowMemoryState ? LowMemReason.LowMemStateSimulation : LowMemReason.BackToNormalSimulation,
                availableMemForLog,
                -2,
                memInfoForLog.TotalPhysicalMemory.GetValue(SizeUnit.Bytes),
                memInfoForLog.CurrentCommitCharge.GetValue(SizeUnit.Bytes));
            if (_logger.IsInfoEnabled)
                _logger.Info("Simulating : " + (LowMemoryState ? "Low memory event" : "Back to normal memory usage"));
            RunLowMemoryHandlers(LowMemoryState);
        }

        internal int CheckMemoryStatus(SmapsReader smapsReader)
        {
            int timeout;
            bool isLowMemory;
            long totalUnmanagedAllocations;
            (Size AvailableMemory, Size TotalPhysicalMemory, Size CurrentCommitCharge) stats;
            try
            {
                isLowMemory = GetLowMemory(out totalUnmanagedAllocations, out stats, smapsReader);
            }
            catch (OutOfMemoryException)
            {
                isLowMemory = true;
                stats = (new Size(), new Size(), new Size());
                totalUnmanagedAllocations = -1;
            }
            if (isLowMemory)
            {
                if (LowMemoryState == false)
                {
                    try
                    {
                        if (_logger.IsInfoEnabled)
                        {

                            _logger.Info("Low memory detected, will try to reduce memory usage...");

                        }
                        AddLowMemEvent(LowMemReason.LowMemOnTimeoutChk,
                            stats.AvailableMemory.GetValue(SizeUnit.Bytes),
                            totalUnmanagedAllocations,
                            stats.TotalPhysicalMemory.GetValue(SizeUnit.Bytes),
                            stats.CurrentCommitCharge.GetValue(SizeUnit.Bytes));
                    }
                    catch (OutOfMemoryException)
                    {
                        // nothing we can do, we'll wait and try again
                    }
                }
                LowMemoryState = true;
                _clearInactiveHandlersCounter = 0;
                RunLowMemoryHandlers(true);
                timeout = 500;
            }
            else
            {
                if (LowMemoryState)
                {
                    if (_logger.IsInfoEnabled)
                        _logger.Info("Back to normal memory usage detected");
                    AddLowMemEvent(LowMemReason.BackToNormal,
                        stats.AvailableMemory.GetValue(SizeUnit.Bytes),
                        totalUnmanagedAllocations,
                        stats.TotalPhysicalMemory.GetValue(SizeUnit.Bytes),
                        stats.CurrentCommitCharge.GetValue(SizeUnit.Bytes));
                }
                LowMemoryState = false;
                RunLowMemoryHandlers(false);
                timeout = stats.AvailableMemory < _lowMemoryThreshold * 2 ? 1000 : 5000;
            }

            return timeout;
        }

        private bool GetLowMemory(out long totalUnmanagedAllocations,
            out (Size AvailableMemory, Size TotalPhysicalMemory, Size CurrentCommitCharge) memStats,
            SmapsReader smapsReader)
        {
            totalUnmanagedAllocations = 0;
            if (++_clearInactiveHandlersCounter > 60) // 5 minutes == WaitAny 5 Secs * 60
            {
                _clearInactiveHandlersCounter = 0;
                ClearInactiveHandlers();
            }

            foreach (var stats in NativeMemory.ThreadAllocations.Values)
            {
                if (stats.IsThreadAlive() == false)
                    continue;

                totalUnmanagedAllocations += stats.TotalAllocated;
            }

            var memInfo = MemoryInformation.GetMemoryInfo();
            var isLowMemory = IsLowMemory(memInfo, smapsReader, out _);

            // memInfo.AvailableMemory is updated in IsLowMemory for Linux (adding shared clean)
            memStats = (memInfo.AvailableMemory, memInfo.TotalPhysicalMemory, memInfo.CurrentCommitCharge);
            return isLowMemory;
        }

        public bool IsLowMemory(MemoryInfoResult memInfo, SmapsReader smapsReader, out long sharedCleanInBytes)
        {
            if (OS.IsLinux)
            {
                var result = smapsReader.CalculateMemUsageFromSmaps<SmapsReaderNoAllocResults>();
                memInfo.AvailableMemory.Add(result.SharedClean, SizeUnit.Bytes);
                sharedCleanInBytes = result.SharedClean;
            }
            else
            {
                sharedCleanInBytes = 0;
            }

            // We consider low memory only if we don't have enough free pyhsical memory or
            // the commited memory size if larger than our pyhsical memory.
            // This is to ensure that from one hand we don't hit the disk to do page faults and from the other hand
            // we don't want to stay in low memory due to retained memory.
            var isLowMemory = memInfo.AvailableMemory < _lowMemoryThreshold;

            if (OS.IsPosix == false)
            {
                // this is relevant only on Windows
                var commitChargePlusMinSizeToKeepFree = memInfo.CurrentCommitCharge + GetCommitChargeThreshold(memInfo);
                isLowMemory |= memInfo.TotalCommittableMemory <= commitChargePlusMinSizeToKeepFree;
            }

            return isLowMemory;
        }

        public Size LowMemoryThreshold => _lowMemoryThreshold;

        public Size GetCommitChargeThreshold(MemoryInfoResult memInfo)
        {
            return memInfo.TotalCommittableMemory * _commitChargeThreshold;
        }

        private void AddLowMemEvent(LowMemReason reason, long availableMem, long totalUnmanaged, long physicalMem, long currentcommitCharge)
        {
            var lowMemEventDetails = new LowMemEventDetails
            {
                Reason = reason,
                FreeMem = availableMem,
                TotalUnmanaged = totalUnmanaged,
                PhysicalMem = physicalMem,
                LowMemThreshold = _lowMemoryThreshold.GetValue(SizeUnit.Bytes),
                CurrentCommitCharge = currentcommitCharge,
                Time = DateTime.UtcNow
            };

            LowMemEventDetailsStack[_lowMemEventDetailsIndex++] = lowMemEventDetails;
            if (_lowMemEventDetailsIndex == LowMemEventDetailsStack.Length)
                _lowMemEventDetailsIndex = 0;
        }

        public void SimulateLowMemoryNotification()
        {
            _simulatedLowMemory.Set();
        }
    }
}

