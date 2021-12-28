using FASTER.core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebCore.Cache
{
    public class KVCache : IDisposable
    {
        private readonly IDevice _log;
        private readonly IDevice _obj;
        private readonly FasterKV<Md5Key, DataValue> _store;
        private readonly ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> _session;

        /// <summary></summary>
        /// <param name="cacheDirectory"></param>
        /// <param name="size">size of hash table in #cache lines; 64 bytes per cache line</param>
        /// <param name="pageSizeBits">Size of a segment (group of pages), in bits: 9, 15, 22</param>
        /// <param name="memorySizeBits">Total size of in-memory part of log, in bits: 14, 20, 30</param>
        /// <param name="mutableFraction">Fraction of log marked as mutable (in-place updates): 0.1, 0.2, 0.3</param>
        public KVCache(string cacheDirectory, long size = 1L << 20, int pageSizeBits = 9, int memorySizeBits = 14, double mutableFraction = 0.1)
        {
            _log = Devices.CreateLogDevice(Path.Combine(cacheDirectory, "cache.log"));
            _obj = Devices.CreateLogDevice(Path.Combine(cacheDirectory, "cache.obj.log"));
            var checkpointDir = new DirectoryInfo(Path.Combine(cacheDirectory, "checkpoints"));

            var logSettings = new LogSettings
            {
                LogDevice = _log,
                ObjectLogDevice = _obj,
                MutableFraction = mutableFraction,
                MemorySizeBits = memorySizeBits,
                PageSizeBits = pageSizeBits
            };

            var checkpointSettings = new CheckpointSettings
            {
                CheckpointDir = checkpointDir.FullName
            };

            var serializerSettings = new SerializerSettings<Md5Key, DataValue>
            {
                keySerializer = () => new Md5KeySerializer(),
                valueSerializer = () => new DataValueSerializer()
            };

            _store = new FasterKV<Md5Key, DataValue>(size, logSettings, checkpointSettings, serializerSettings);

            if (checkpointDir.Exists)
            {
                var files = checkpointDir.GetFiles();
                if (files.Length > 0)
                {
                    _store.Recover();
                }
                foreach (var file in files)
                {
                    file.Delete();
                }
            }

            _session = _store.NewSession(new SimpleFunctions<Md5Key, DataValue>());
        }

        public byte[] Get(string key)
        {
            var (status, output) = _session.Read(new Md5Key(key));

            return status == Status.OK ? output.Value : default;
        }

        public async ValueTask<byte[]> GetAsync(string key)
        {
            var result = await _session.ReadAsync(new Md5Key(key));

            var (status, output) = result.Complete();

            return status == Status.OK ? output.Value : default;
        }

        public void Set(string key, byte[] value)
        {
            _session.Upsert(new Md5Key(key), new DataValue
            {
                Value = value
            });
        }

        public async Task SetAsync(string key, byte[] value)
        {
            var result = await _session.RMWAsync(new Md5Key(key), new DataValue
            {
                Value = value
            });

            await result.CompleteAsync();
        }

        public bool SaveSnapshot(bool saveHybridLog = true)
        {
            bool success = saveHybridLog
                ? _store.TakeHybridLogCheckpoint(out _)
                : _store.TakeFullCheckpoint(out _);

            _store.CompleteCheckpointAsync().GetAwaiter().GetResult();

            return success;
        }

        public async ValueTask<bool> SaveSnapshotAsync(bool saveHybridLog = true)
        {
            (bool success, _) = saveHybridLog
                ? await _store.TakeHybridLogCheckpointAsync(CheckpointType.Snapshot)
                : await _store.TakeFullCheckpointAsync(CheckpointType.Snapshot);

            await _store.CompleteCheckpointAsync();

            return success;
        }

        public void Dispose()
        {
            _store.Dispose();
            _log.Dispose();
            _obj.Dispose();
        }
    }
}
