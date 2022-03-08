using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebInterface
{
    /// <summary>Allow to raise a task completion source with minimal costs and attempt to avoid stalls due to thread pool starvation. </summary>
    public interface ITaskExecutor
    {
        /// <summary>
        /// Execute callback only once
        /// </summary>
        /// <param name="callback">Represents a callback method to be executed by a thread pool thread</param>
        /// <param name="state">Parameter for callback</param>
        /// <param name="laterOnEvent">later on event</param>
        void Execute(WaitCallback callback, object state, bool laterOnEvent = true);

        /// <summary>
        /// Complete a task
        /// </summary>
        /// <param name="task">Represents the producer side of a Task unbound to a delegate, providing access to the consumer side through the Task Completion Source</param>
        /// <param name="result"></param>
        void Complete(TaskCompletionSource<object> task, object result = null);

        /// <summary>
        /// Complete a task and replace a result
        /// </summary>
        /// <param name="task">Represents the producer side of a Task unbound to a delegate, providing access to the consumer side through the Task Completion Source</param>
        /// <param name="result"></param>
        void CompleteAndReplace(ref TaskCompletionSource<object> task, object result = null);

        /// <summary>
        /// Complete a task and continue with a result
        /// </summary>
        /// <param name="task">Represents the producer side of a Task unbound to a delegate, providing access to the consumer side through the Task Completion Source</param>
        /// <param name="act"></param>
        /// <param name="result"></param>
        void CompleteAndContinueWith(ref TaskCompletionSource<object> task, Action act, object result = null);
    }
}
