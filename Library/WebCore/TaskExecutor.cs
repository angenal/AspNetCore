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
    }
}
