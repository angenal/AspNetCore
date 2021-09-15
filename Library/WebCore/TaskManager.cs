using FluentScheduler;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WebInterface;

namespace WebCore
{
    public class TaskManager : ITaskManager
    {
        public static TaskManager Default = new TaskManager();

        private readonly SemaphoreSlim signal = new SemaphoreSlim(0);
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> tasks = new ConcurrentQueue<Func<CancellationToken, Task>>();

        public void Enqueue(Func<CancellationToken, Task> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            tasks.Enqueue(task);
            signal.Release();
        }

        public async Task<Func<CancellationToken, Task>> Dequeue(CancellationToken cancellationToken)
        {
            await signal.WaitAsync(cancellationToken);
            tasks.TryDequeue(out var task);
            return task;
        }

        public void RunOnceAt(Action job, DateTime time)
        {
            JobManager.AddJob(job, s => s.ToRunOnceAt(time));
        }
    }
}
