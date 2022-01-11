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
        private readonly ConcurrentQueue<(WaitCallback, object)> _actions1 = new ConcurrentQueue<(WaitCallback, object)>();
        private readonly ConcurrentQueue<(WaitCallback, Parameter)> _actions2 = new ConcurrentQueue<(WaitCallback, Parameter)>();

        private readonly ManualResetEventSlim _event1 = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _event2 = new ManualResetEventSlim(false);

        private void Run1()
        {
            Utils.NativeMemory.EnsureRegistered();

            int tries = 0, count = 1 + Environment.ProcessorCount;
            while (true)
            {
                while (_actions1.TryDequeue(out (WaitCallback callback, object state) result))
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
                    _event1.WaitHandle.WaitOne();
                    _event1.Reset();

                    // Nothing we can do here, just block.
                    tries = 0;
                }
            }
        }

        private void Run2()
        {
            Utils.NativeMemory.EnsureRegistered();

            int tries = 0, count = 1 + Environment.ProcessorCount;
            while (true)
            {
                while (_actions2.TryDequeue(out (WaitCallback callback, Parameter state) result))
                {
                    while (result.state.Time > DateTimeOffset.Now.ToUnixTimeSeconds()) { Thread.Sleep(1000); }
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
                    _event2.WaitHandle.WaitOne();
                    _event2.Reset();

                    // Nothing we can do here, just block.
                    tries = 0;
                }
            }
        }

        /// <summary></summary>
        public void Enqueue(WaitCallback callback, object state, bool laterOnEvent = false)
        {
            if (laterOnEvent)
            {
                _actions2.Enqueue((callback, new Parameter { Time = 1 + DateTimeOffset.Now.ToUnixTimeSeconds(), State = state }));
                _event2.Set();
            }
            else
            {
                _actions1.Enqueue((callback, state));
                _event1.Set();
            }
        }

        /// <summary></summary>
        public Runner()
        {
            new Thread(Run1)
            {
                IsBackground = true,
                Name = "Runner1"
            }.Start();
            new Thread(Run2)
            {
                IsBackground = true,
                Name = "Runner2"
            }.Start();
        }

        /// <summary></summary>
        internal class Parameter
        {
            internal long Time { get; set; }
            internal object State { get; set; }
        }
    }
}
