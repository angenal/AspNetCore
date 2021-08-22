using System;
using System.Linq;
using System.Threading;
using Hangfire.LiteDB.Entities;
using Hangfire.Logging;
using Hangfire.Server;
using LiteDB;

namespace Hangfire.LiteDB
{
    /// <summary>
    /// Represents Counter collection aggregator for LiteDB database
    /// </summary>
    [Obsolete]
    public class CountersAggregator : IBackgroundProcess, IServerComponent
    {
        private static readonly ILog Logger = LogProvider.For<CountersAggregator>();

        private const int NumberOfRecordsInSinglePass = 1000;
        private static readonly TimeSpan DelayBetweenPasses = TimeSpan.FromMilliseconds(500);

        private readonly LiteDbStorage _storage;
        private readonly TimeSpan _interval;

        /// <summary>
        /// Constructs Counter collection aggregator
        /// </summary>
        /// <param name="storage">LiteDB storage</param>
        /// <param name="interval">Checking interval</param>
        public CountersAggregator(LiteDbStorage storage, TimeSpan interval)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _interval = interval;
        }

        /// <summary>
        /// Runs aggregator
        /// </summary>
        /// <param name="context">Background processing context</param>
        [Obsolete]
        public void Execute(BackgroundProcessContext context)
        {
            Execute(context.CancellationToken);
        }

        /// <summary>
        /// Runs aggregator
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public void Execute(CancellationToken cancellationToken)
        {
            Logger.DebugFormat("Aggregating records in 'Counter' table...");

            long removedCount = 0;

            do
            {
                using (var storageConnection = (LiteDbConnection)_storage.GetConnection())
                {
                    var database = storageConnection.Database;

                    var recordsToAggregate = database
                        .StateDataCounter
                        .FindAll()
                        .Take(NumberOfRecordsInSinglePass)
                        .ToList();

                    var recordsToMerge = recordsToAggregate
                        .GroupBy(_ => _.Key).Select(_ => new
                        {
                            _.Key,
                            Value = _.Sum(x => x.Value.ToInt64()),
                            ExpireAt = _.Max(x => x.ExpireAt)
                        });

                    foreach (var id in recordsToAggregate.Select(_ => _.Id))
                    {
                        database
                            .StateDataCounter
                            .Delete(id);
                        removedCount++;
                    }

                    foreach (var item in recordsToMerge)
                    {
                        AggregatedCounter aggregatedItem = database
                            .StateDataAggregatedCounter
                            .Find(_ => _.Key == item.Key)
                            .FirstOrDefault();

                        if (aggregatedItem != null)
                        {
                            var aggregatedCounters = database.StateDataAggregatedCounter.Find(_ => _.Key == item.Key);

                            foreach (var counter in aggregatedCounters)
                            {
                                counter.Value = counter.Value.ToInt64() + item.Value;
                                counter.ExpireAt = item.ExpireAt > aggregatedItem.ExpireAt
                                    ?  (item.ExpireAt.HasValue ? (DateTime?)item.ExpireAt.Value : null)
                                    :  (aggregatedItem.ExpireAt.HasValue ? (DateTime?)aggregatedItem.ExpireAt.Value : null);
                                database.StateDataAggregatedCounter.Update(counter);
                            }
                        }
                        else
                        {
                            database
                                .StateDataAggregatedCounter
                                .Insert(new AggregatedCounter 
                                {
                                    Id = ObjectId.NewObjectId(),
                                    Key = item.Key,
                                    Value = item.Value,
                                    ExpireAt = item.ExpireAt
                                });
                        }
                    }
                }

                if (removedCount >= NumberOfRecordsInSinglePass)
                {
                    cancellationToken.WaitHandle.WaitOne(DelayBetweenPasses);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            } while (removedCount >= NumberOfRecordsInSinglePass);

            cancellationToken.WaitHandle.WaitOne(_interval);
        }

        /// <summary>
        /// Returns text representation of the object
        /// </summary>
        public override string ToString()
        {
            return "LiteDB Counter Collection Aggregator";
        }
    }
}