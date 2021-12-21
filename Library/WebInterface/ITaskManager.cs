using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebInterface
{
    /// <summary>Provides task queue manager methods. </summary>
    public interface ITaskManager
    {
        /// <summary>
        /// Adds an task to the end of the concurrent queue.
        /// </summary>
        void Enqueue(Func<CancellationToken, Task> task);

        /// <summary>
        /// Tries to remove and return the task at the beginning of the concurrent queue.
        /// </summary>
        Task<Func<CancellationToken, Task>> Dequeue(CancellationToken cancellationToken);

        /// <summary>
        /// Adds a job schedule to the job manager, runs the job once at the given time.
        /// </summary>
        void RunOnceAt(Action job, DateTime time, string name = null, params Action[] andThenJobs);

        /// <summary>
        /// Adds a job schedule to the job manager, runs the job according to the given interval.
        /// </summary>
        void RunEvery(Action job, int interval, string name = null, params Action[] andThenJobs);

        /// <summary>
        /// Runs the job now.
        /// </summary>
        void RunNow(Action job);

        /// <summary>
        /// Removes the schedule of the given name.
        /// </summary>
        void RemoveJob(string name);

        /// <summary>
        /// Removes all schedules.
        /// </summary>
        void RemoveAllJobs();
    }
}
