using System;
using System.Threading.Tasks;

namespace WebCore
{
    /// <summary>Provides task helper methods. </summary>
    public static class TaskUtilities
    {
        private static Task _completedTask = null;

        /// <summary>Gets a completed task without result type. </summary>
        public static Task CompletedTask
        {
            get
            {
                if (_completedTask == null)
                {
                    lock (typeof(TaskUtilities))
                    {
                        if (_completedTask == null)
                        {
#if !LEGACY
                            _completedTask = Task.FromResult<object>(null);
#else
                            var source = new TaskCompletionSource<object>();
                            source.SetResult(null);
                            _completedTask = source.Task;
#endif
                        }
                    }
                }
                return _completedTask;
            }
        }

        /// <summary>Converts a callback based asynchronous method into a task. </summary>
        /// <param name="func">The function. </param>
        /// <returns>The task. </returns>
        public static Task RunCallbackMethodAsync(Action<Action> func)
        {
            var task = new TaskCompletionSource<object>();
            func(() => task.SetResult(new object()));
            return task.Task;
        }

        /// <summary>Converts a callback based asynchronous method into a task. </summary>
        /// <param name="func">The function. </param>
        /// <returns>The task. </returns>
        public static Task<TResult> RunCallbackMethodAsync<TResult>(Action<Action<TResult>> func)
        {
            var task = new TaskCompletionSource<TResult>();
            func(task.SetResult);
            return task.Task;
        }

        /// <summary>Converts a callback based asynchronous method into a task. </summary>
        /// <param name="func">The function. </param>
        /// <param name="param1">The first parameter. </param>
        /// <returns>The task. </returns>
        public static Task RunCallbackMethodAsync<T1>(Action<T1, Action> func, T1 param1)
        {
            var task = new TaskCompletionSource<object>();
            func(param1, () => task.SetResult(new object()));
            return task.Task;
        }

        /// <summary>Converts a callback based asynchronous method into a task. </summary>
        /// <param name="func">The function. </param>
        /// <param name="param1">The first parameter. </param>
        /// <returns>The task. </returns>
        public static Task<TResult> RunCallbackMethodAsync<T1, TResult>(Action<T1, Action<TResult>> func, T1 param1)
        {
            var task = new TaskCompletionSource<TResult>();
            func(param1, task.SetResult);
            return task.Task;
        }

        /// <summary>Converts a callback based asynchronous method into a task. </summary>
        /// <param name="func">The function. </param>
        /// <param name="param1">The first parameter. </param>
        /// <param name="param2">The second parameter. </param>
        /// <returns>The task. </returns>
        public static Task RunCallbackMethodAsync<T1, T2>(Action<T1, T2, Action> func, T1 param1, T2 param2)
        {
            var task = new TaskCompletionSource<object>();
            func(param1, param2, () => task.SetResult(new object()));
            return task.Task;
        }

        /// <summary>Converts a callback based asynchronous method into a task. </summary>
        /// <param name="func">The function. </param>
        /// <param name="param1">The first parameter. </param>
        /// <param name="param2">The second parameter. </param>
        /// <returns>The task. </returns>
        public static Task<TResult> RunCallbackMethodAsync<T1, T2, TResult>(Action<T1, T2, Action<TResult>> func, T1 param1, T2 param2)
        {
            var task = new TaskCompletionSource<TResult>();
            func(param1, param2, task.SetResult);
            return task.Task;
        }

        /// <summary>Converts a callback based asynchronous method into a task. </summary>
        /// <param name="func">The function. </param>
        /// <param name="param1">The first parameter. </param>
        /// <param name="param2">The second parameter. </param>
        /// <param name="param3">The third parameter. </param>
        /// <returns>The task. </returns>
        public static Task RunCallbackMethodAsync<T1, T2, T3>(Action<T1, T2, T3, Action> func, T1 param1, T2 param2, T3 param3)
        {
            var task = new TaskCompletionSource<object>();
            func(param1, param2, param3, () => task.SetResult(new object()));
            return task.Task;
        }

        /// <summary>Converts a callback based asynchronous method into a task. </summary>
        /// <param name="func">The function. </param>
        /// <param name="param1">The first parameter. </param>
        /// <param name="param2">The second parameter. </param>
        /// <param name="param3">The third parameter. </param>
        /// <returns>The task. </returns>
        public static Task<TResult> RunCallbackMethodAsync<T1, T2, T3, TResult>(Action<T1, T2, T3, Action<TResult>> func, T1 param1, T2 param2, T3 param3)
        {
            var task = new TaskCompletionSource<TResult>();
            func(param1, param2, param3, task.SetResult);
            return task.Task;
        }
    }

    /// <summary>Synchronizes tasks so that they are executed after each other.</summary>
    /// <typeparam name="T">The return type of the task.</typeparam>
    public class TaskSynchronizationScope
    {
        private Task _currentTask;
        private readonly object _lock = new object();

        /// <summary>Executes the given task when the previous task has been completed.</summary>
        /// <param name="task">The task function.</param>
        /// <returns>The task.</returns>
        public Task RunAsync(Func<Task> task)
        {
            return RunAsync<object>(async () =>
            {
                await task();
                return null;
            });
        }

        /// <summary>Executes the given task when the previous task has been completed.</summary>
        /// <param name="task">The task function.</param>
        /// <returns>The task.</returns>
        public Task<T> RunAsync<T>(Func<Task<T>> task)
        {
            lock (_lock)
            {
                if (_currentTask == null)
                {
                    var currentTask = task();
                    _currentTask = currentTask;
                    return currentTask;
                }
                else
                {
                    var source = new TaskCompletionSource<T>();
                    _currentTask.ContinueWith(t =>
                    {
                        var nextTask = task();
                        nextTask.ContinueWith(nt =>
                        {
                            if (nt.IsCompleted)
                                source.SetResult(nt.Result);
                            else if (nt.IsFaulted)
                                source.SetException(nt.Exception);
                            else
                                source.SetCanceled();

                            lock (_lock)
                            {
                                if (_currentTask.Status == TaskStatus.RanToCompletion)
                                    _currentTask = null;
                            }
                        });
                    });
                    _currentTask = source.Task;
                    return source.Task;
                }
            }
        }
    }
}
