using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WebInterface;

namespace WebCore
{
    /// <summary>Allow to raise a task completion source with minimal costs and attempt to avoid stalls due to thread pool starvation. </summary>
    public class TaskExecutor : ITaskExecutor
    {
        /// <summary></summary>
        public static TaskExecutor Default = new TaskExecutor();

        private readonly Runner runner = new Runner();

        /// <summary>
        /// Execute callback only once
        /// </summary>
        /// <param name="callback">Represents a callback method to be executed by a thread pool thread</param>
        /// <param name="state"></param>
        public void Execute(WaitCallback callback, object state)
        {
            callback = new RunOnce(callback).Execute;
            runner.Enqueue(callback, state);
            ThreadPool.QueueUserWorkItem(callback, state);
        }

        /// <summary>
        /// Complete a task
        /// </summary>
        /// <param name="task">Represents the producer side of a Task unbound to a delegate, providing access to the consumer side through the Task Completion Source</param>
        /// <param name="result"></param>
        public void Complete(TaskCompletionSource<object> task, object result = null)
        {
            task.TrySetResult(result);
        }

        /// <summary>
        /// Complete a task and replace a result
        /// </summary>
        /// <param name="task">Represents the producer side of a Task unbound to a delegate, providing access to the consumer side through the Task Completion Source</param>
        /// <param name="result"></param>
        public void CompleteAndReplace(ref TaskCompletionSource<object> task, object result = null)
        {
            var task2 = Interlocked.Exchange(ref task, new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously));
            task2.TrySetResult(result);
        }

        /// <summary>
        /// Complete a task and continue with a result
        /// </summary>
        /// <param name="task">Represents the producer side of a Task unbound to a delegate, providing access to the consumer side through the Task Completion Source</param>
        /// <param name="act"></param>
        /// <param name="result"></param>
        public void CompleteAndContinueWith(ref TaskCompletionSource<object> task, Action act, object result = null)
        {
            var task2 = Interlocked.Exchange(ref task, new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously));
            Execute(state =>
            {
                var (tcs, action) = ((TaskCompletionSource<object>, Action))state;
                tcs.TrySetResult(result);
                act();
            }, (task2, act));
        }

        /// <summary></summary>
        class RunOnce
        {
            private WaitCallback _callback;

            public RunOnce(WaitCallback callback)
            {
                _callback = callback;
            }

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

        /// <summary></summary>
        class Runner
        {
            private const string TasksExecuterThreadName = "TaskExecuter";
            private readonly ConcurrentQueue<(WaitCallback, object)> _actions = new ConcurrentQueue<(WaitCallback, object)>();

            private readonly ManualResetEventSlim _event = new ManualResetEventSlim(false);

            private void Run()
            {
                Utils.NativeMemory.EnsureRegistered();

                int tries = 0;
                while (true)
                {
                    while (_actions.TryDequeue(out (WaitCallback callback, object state) result))
                    {
                        try
                        {
                            result.callback(result.state);
                        }
                        finally { }
                    }

                    // PERF: Entering a kernel lock even if the ManualResetEventSlim will try to avoid that doing some spin locking
                    //       is very costly. This is a hack that is allowing amortize a bit very high frequency events. The proper
                    if (tries < 5)
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
    }
}
