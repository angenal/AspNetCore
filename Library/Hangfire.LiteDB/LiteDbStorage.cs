using System;
using System.Collections.Generic;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
namespace Hangfire.LiteDB
{
    /// <summary>
    /// 
    /// </summary>
    public class LiteDbStorage : JobStorage
    {
        private readonly string _connectionString;

        private readonly LiteDbStorageOptions _storageOptions;

        /// <summary>
        /// Constructs Job Storage by database connection string
        /// </summary>
        /// <param name="connectionString">LiteDB connection string</param>
        public LiteDbStorage(string connectionString)
            : this(connectionString, new LiteDbStorageOptions())
        {
        }

        /// <summary>
        /// Constructs Job Storage by database connection string and options
        /// </summary>
        /// <param name="connectionString">LiteDB connection string</param>
        /// <param name="storageOptions">Storage options</param>
        public LiteDbStorage(string connectionString, LiteDbStorageOptions storageOptions)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connectionString = connectionString;
            _storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));

            Connection = HangfireDbContext.Instance(connectionString, storageOptions.Prefix);
            Connection.Init(_storageOptions);
            var defaultQueueProvider = new LiteDbJobQueueProvider(_storageOptions);
            QueueProviders = new PersistentJobQueueProviderCollection(defaultQueueProvider);
        }

        /// <summary>
        /// Database context
        /// </summary>
        public HangfireDbContext Connection { get; }

        /// <summary>
        /// Queue providers collection
        /// </summary>
        public PersistentJobQueueProviderCollection QueueProviders { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IMonitoringApi GetMonitoringApi()
        {

            return new LiteDbMonitoringApi(Connection, QueueProviders);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IStorageConnection GetConnection()
        {
            return new LiteDbConnection(Connection, _storageOptions, QueueProviders);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public override void WriteOptionsToLog(ILog logger)
        {
            //logger.Info("Using the following options for LiteDB job storage:");
        }

        /// <summary>
        /// Opens connection to database
        /// </summary>
        /// <returns>Database context</returns>
        public HangfireDbContext CreateAndOpenConnection()
        {
            return _connectionString != null
                ? HangfireDbContext.Instance(_connectionString, _storageOptions.Prefix)
                : null;
        }

        /// <summary>
        /// Returns text representation of the object
        /// </summary>
        public override string ToString()
        {
            return _storageOptions.DashboardTitle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<IServerComponent> GetComponents()
        {
            yield return new ExpirationManager(this, _storageOptions.JobExpirationCheckInterval);
            yield return new CountersAggregator(this, _storageOptions.CountersAggregateInterval);
        }
    }
}
