using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace WebCore
{
    /// <summary>Run only once.</summary>
    public class RunOnce
    {
        private WaitCallback _callback;

        /// <summary></summary>
        public RunOnce(WaitCallback callback)
        {
            _callback = callback;
        }

        /// <summary></summary>
        public void Execute(object state)
        {
            var callback = _callback;
            if (callback == null)
                return;

            if (Interlocked.CompareExchange(ref _callback, null, callback) != callback)
                return;

            callback(state);
        }
    }

    /// <summary>Runner is a bit very high frequency events.</summary>
    public class Runner
    {
        private const string TasksExecuterThreadName = "Runner";
        private readonly ConcurrentQueue<(WaitCallback, object)> _actions = new ConcurrentQueue<(WaitCallback, object)>();

        private readonly ManualResetEventSlim _event = new ManualResetEventSlim(false);

        private void Run()
        {
            Utils.NativeMemory.EnsureRegistered();

            int tries = 0, count = 1 + Environment.ProcessorCount;
            while (true)
            {
                while (_actions.TryDequeue(out (WaitCallback callback, object state) result))
                {
                    try { result.callback(result.state); }
                    catch (Exception e) { Trace.TraceError(e.ToString()); }
                }

                // PERF: Entering a kernel lock even if the ManualResetEventSlim will try to avoid that doing some spin locking is very costly.
                //       This is a hack that is allowing amortize a bit very high frequency events.
                if (tries < count)
                {
                    // Yield execution quantum. If we are in a high-frequency event we will be able to avoid the kernel lock.
                    Thread.Sleep(0);
                    tries++;
                }
                else
                {
                    _event.WaitHandle.WaitOne();
                    _event.Reset();

                    // Nothing we can do here, just block.
                    tries = 0;
                }
            }
        }

        public void Enqueue(WaitCallback callback, object state)
        {
            _actions.Enqueue((callback, state));
            _event.Set();
        }

        public Runner()
        {
            new Thread(Run)
            {
                IsBackground = true,
                Name = TasksExecuterThreadName
            }.Start();
        }
    }

    /// <summary>Runner is a bit very high frequency delegate events.</summary>
    public class RunnerDelegate
    {
        private const string TasksExecuterThreadName = "RunnerDelegate";
        private readonly ConcurrentQueue<(WaitCallback, DelegateObject)> _actions = new ConcurrentQueue<(WaitCallback, DelegateObject)>();

        private readonly ManualResetEventSlim _event = new ManualResetEventSlim(false);

        private void Run()
        {
            Utils.NativeMemory.EnsureRegistered();

            int tries = 0, count = 1 + Environment.ProcessorCount;
            while (true)
            {
                long time = DateTimeOffset.Now.ToUnixTimeSeconds();
                while (_actions.TryDequeue(out (WaitCallback callback, DelegateObject state) result))
                {
                    while (result.state.Time > time) Thread.Sleep(1);
                    try { result.callback(result.state.State); }
                    catch (Exception e) { Trace.TraceError(e.ToString()); }
                }

                // PERF: Entering a kernel lock even if the ManualResetEventSlim will try to avoid that doing some spin locking is very costly.
                //       This is a hack that is allowing amortize a bit very high frequency events.
                if (tries < count)
                {
                    // Yield execution quantum. If we are in a high-frequency event we will be able to avoid the kernel lock.
                    Thread.Sleep(0);
                    tries++;
                }
                else
                {
                    _event.WaitHandle.WaitOne();
                    _event.Reset();

                    // Nothing we can do here, just block.
                    tries = 0;
                }
            }
        }

        public void Enqueue(WaitCallback callback, object state)
        {
            _actions.Enqueue((callback, new DelegateObject { Time = 1 + DateTimeOffset.Now.ToUnixTimeSeconds(), State = state }));
            _event.Set();
        }

        public RunnerDelegate()
        {
            new Thread(Run)
            {
                IsBackground = true,
                Name = TasksExecuterThreadName
            }.Start();
        }

        internal class DelegateObject
        {
            internal long Time { get; set; }
            internal object State { get; set; }
        }
    }
}
