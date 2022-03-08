using System;
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

        private readonly Runner1[] r1;
        private readonly Runner2[] r2;
        private readonly int c = 0;
        private int i1 = 0, i2 = 0;

        /// <summary></summary>
        public TaskExecutor()
        {
            c = Environment.ProcessorCount;
            r1 = new Runner1[c];
            r2 = new Runner2[c];
            for (int i = 0; i < c; i++)
            {
                r1[i] = new Runner1();
                r2[i] = new Runner2();
            }
        }

        /// <summary>
        /// Execute callback only once
        /// </summary>
        /// <param name="callback">Represents a callback method to be executed by a thread pool thread</param>
        /// <param name="state">Parameter for callback</param>
        /// <param name="laterOnEvent">later on event</param>
        public void Execute(WaitCallback callback, object state, bool laterOnEvent = true)
        {
            callback = new RunOnce(callback).Execute;
            if (laterOnEvent == false)
            {
                int i = Math.Abs(Interlocked.Increment(ref i1) % c);
                r1[i].Enqueue(callback, state);
            }
            else
            {
                int i = Math.Abs(Interlocked.Increment(ref i2) % c);
                r2[i].Enqueue(callback, state);
            }
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
    }
}
