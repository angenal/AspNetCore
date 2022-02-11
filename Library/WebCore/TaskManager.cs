using FluentScheduler;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WebInterface;

namespace WebCore
{
    /// <summary>Provides task queue manager methods. </summary>
    public class TaskManager : ITaskManager
    {
        /// <summary></summary>
        public static TaskManager Default = new TaskManager();

        private readonly SemaphoreSlim signal = new SemaphoreSlim(0);
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> tasks = new ConcurrentQueue<Func<CancellationToken, Task>>();

        /// <summary>
        /// Adds an task to the end of non blocking concurrency queue.
        /// </summary>
        public void Enqueue(Action job)
        {
            JobManager.AddJob(job, s => s.ToRunNow());
        }

        /// <summary>
        /// Adds an task to the end of the concurrent queue.
        /// </summary>
        public void Enqueue(Func<CancellationToken, Task> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            tasks.Enqueue(task);
            signal.Release();
        }

        /// <summary>
        /// Tries to remove and return the task at the beginning of the concurrent queue.
        /// </summary>
        public async Task<Func<CancellationToken, Task>> Dequeue(CancellationToken cancellationToken)
        {
            await signal.WaitAsync(cancellationToken);
            tasks.TryDequeue(out var task);
            return task;
        }

        /// <summary>
        /// Adds a job schedule to the job manager, runs the job once at the given time.
        /// </summary>
        public void RunOnceAt(Action job, DateTime time, string name = null, params Action[] andThenJobs)
        {
            JobManager.AddJob(job, s =>
            {
                if (!string.IsNullOrEmpty(name)) s = s.WithName(name);
                foreach (Action andThenJob in andThenJobs) s = s.AndThen(andThenJob);
                s.ToRunOnceAt(time);
            });
        }

        /// <summary>
        /// Adds a job schedule to the job manager, runs the job according to the given interval.
        /// </summary>
        public void RunEvery(Action job, int interval, string name = null, params Action[] andThenJobs)
        {
            JobManager.AddJob(job, s =>
            {
                if (!string.IsNullOrEmpty(name)) s = s.WithName(name);
                foreach (Action andThenJob in andThenJobs) s = s.AndThen(andThenJob);
                s.ToRunEvery(interval);
            });
        }

        /// <summary>
        /// Runs the job now.
        /// </summary>
        public void RunNow(Action job, string name = null)
        {
            JobManager.AddJob(job, s =>
            {
                if (!string.IsNullOrEmpty(name)) s = s.WithName(name);
                s.ToRunNow();
            });
        }

        /// <summary>
        /// Removes the schedule of the given name.
        /// </summary>
        public void RemoveJob(string name)
        {
            JobManager.RemoveJob(name);
        }

        /// <summary>
        /// Removes all schedules.
        /// </summary>
        public void RemoveAllJobs()
        {
            JobManager.RemoveAllJobs();
        }
    }
}
