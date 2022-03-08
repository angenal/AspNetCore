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
    public class Runner1
    {
        private readonly ConcurrentQueue<(WaitCallback, object)> _actions = new ConcurrentQueue<(WaitCallback, object)>();

        private readonly ManualResetEventSlim _event = new ManualResetEventSlim(false);

        private readonly ThreadLocal<Thread> ThreadAllocations;

        private void Run()
        {
            //Utils.NativeMemory.EnsureRegistered();
            GC.KeepAlive(ThreadAllocations.Value); // side affecty

            int tries = 0, count = 1 + Environment.ProcessorCount;
            while (true)
            {
                while (_actions.TryDequeue(out (WaitCallback callback, object state) result))
                {
                    try
                    {
                        result.callback(result.state);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(e.ToString());
                    }
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

        /// <summary>Add an event.</summary>
        public void Enqueue(WaitCallback callback, object state)
        {
            _actions.Enqueue((callback, state));
            _event.Set();
        }

        /// <summary></summary>
        public Runner1()
        {
            ThreadAllocations = new ThreadLocal<Thread>(() => new Thread(Run) { Name = nameof(Runner1), IsBackground = true }, trackAllValues: true);
            ThreadAllocations.Value.Start();
        }
    }

    /// <summary>Runner is a bit very high frequency events with a delay of 1 second.</summary>
    public class Runner2
    {
        private readonly ConcurrentQueue<(WaitCallback, Parameter)> _actions = new ConcurrentQueue<(WaitCallback, Parameter)>();

        private readonly ManualResetEventSlim _event = new ManualResetEventSlim(false);

        private readonly ThreadLocal<Thread> ThreadAllocations;

        private void Run()
        {
            GC.KeepAlive(ThreadAllocations.Value); // side affecty

            int tries = 0, count = 1 + Environment.ProcessorCount;
            while (true)
            {
                while (_actions.TryDequeue(out (WaitCallback callback, Parameter state) result))
                {
                    while (result.state.Time > DateTimeOffset.Now.ToUnixTimeSeconds())
                    {
                        Thread.Sleep(1000);
                    }
                    try
                    {
                        Thread.Sleep(0);
                        result.callback(result.state.State);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(e.ToString());
                    }
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

        /// <summary>Add an event with a delay of 1 second.</summary>
        public void Enqueue(WaitCallback callback, object state)
        {
            _actions.Enqueue((callback, new Parameter { Time = 1 + DateTimeOffset.Now.ToUnixTimeSeconds(), State = state }));
            _event.Set();
        }

        /// <summary></summary>
        public Runner2()
        {
            ThreadAllocations = new ThreadLocal<Thread>(() => new Thread(Run) { Name = nameof(Runner2), IsBackground = true }, trackAllValues: true);
            ThreadAllocations.Value.Start();
        }

        /// <summary></summary>
        internal class Parameter
        {
            internal long Time { get; set; }
            internal object State { get; set; }
        }
    }
}
