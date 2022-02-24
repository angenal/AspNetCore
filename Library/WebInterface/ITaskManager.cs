using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebInterface
{
    /// <summary>Provides task queue manager methods. </summary>
    public interface ITaskManager
    {
        /// <summary>
        /// Adds an task to the end of non blocking concurrency queue.
        /// </summary>
        void Enqueue(Action job);

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
        /// Adds a job schedule to the job manager, runs the job once at the given time.
        /// </summary>
        /// <param name="hours">The hours (0 through 23)</param>
        /// <param name="minutes">The minutes (0 through 59)</param>
        void RunOnceAt(Action job, int hours, int minutes, string name = null, params Action[] andThenJobs);

        /// <summary>
        /// Adds a job schedule to the job manager, runs the job once after the given interval.
        /// </summary>
        void RunOnceIn(Action job, int interval, string name = null, params Action[] andThenJobs);

        /// <summary>
        /// Adds a job schedule to the job manager, runs the job according to the given interval.
        /// </summary>
        void RunEvery(Action job, int interval, string name = null, params Action[] andThenJobs);

        /// <summary>
        /// Runs the job now.
        /// </summary>
        void RunNow(Action job, string name = null);

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
