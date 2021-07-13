﻿using System;
using Hangfire.Storage;
using LiteDB;

namespace Hangfire.LiteDB
{
    /// <summary>
    /// 
    /// </summary>
    public class LiteDbFetchedJob : IFetchedJob
    {
         private readonly HangfireDbContext _connection;
        private readonly ObjectId _id;

        private bool _disposed;

        private bool _removedFromQueue;

        private bool _requeued;

        /// <summary>
        /// Constructs fetched job by database connection, identifier, job ID and queue
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <param name="id">Identifier</param>
        /// <param name="jobId">Job ID</param>
        /// <param name="queue">Queue name</param>
        public LiteDbFetchedJob(HangfireDbContext connection, ObjectId id, int? jobId, string queue)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _id = id;
            JobId = jobId.HasValue ? jobId.Value.ToString() : throw new ArgumentNullException(nameof(jobId));
            Queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        /// <summary>
        /// Job ID
        /// </summary>
        public string JobId { get; }

        /// <summary>
        /// Queue name
        /// </summary>
        public string Queue { get; }

        /// <summary>
        /// Removes fetched job from a queue
        /// </summary>
        public void RemoveFromQueue()
        {
            _connection
                .JobQueue
                .Delete(_id);

            _removedFromQueue = true;
        }

        /// <summary>
        /// Puts fetched job into a queue
        /// </summary>
        public void Requeue()
        {
            
            var jobQueue = _connection.JobQueue.FindById(_id);
            jobQueue.FetchedAt = null;
            _connection.JobQueue.Update(jobQueue);
            _requeued = true;
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            if (!_removedFromQueue && !_requeued)
            {
                Requeue();
            }

            _disposed = true;
        }
    }
}