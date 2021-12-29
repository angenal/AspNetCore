using FASTER.core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebCore.Cache
{
    /// <summary>
    /// Faster Key/Value is a fast concurrent persistent log and key-value store with cache for larger-than-memory data.
    /// https://github.com/microsoft/FASTER
    /// </summary>
    public class KVCache : IDisposable
    {
        private readonly IDevice _log;
        private readonly IDevice _obj;
        private readonly FasterKV<Md5Key, DataValue> _fht;
        private readonly ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> _session;

        /// <summary>
        /// Faster Key/Value in-memory and disk cache store
        /// </summary>
        /// <param name="cacheDirectory">the directory path of cache</param>
        /// <param name="size">size of hash table in #cache lines; 64 bytes per cache line</param>
        /// <param name="pageSizeBits">Size of a segment (group of pages), in bits: 9, 15, 22</param>
        /// <param name="memorySizeBits">Total size of in-memory part of log, in bits: 14, 20, 30</param>
        /// <param name="mutableFraction">Fraction of log marked as mutable (in-place updates): 0.1, 0.2, 0.3</param>
        public KVCache(string cacheDirectory = null, long size = 1L << 20, int pageSizeBits = 9, int memorySizeBits = 14, double mutableFraction = 0.1)
        {
            if (string.IsNullOrEmpty(cacheDirectory)) cacheDirectory = Path.GetTempPath();
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

            _fht = new FasterKV<Md5Key, DataValue>(size, logSettings, checkpointSettings, serializerSettings);

            if (checkpointDir.Exists)
            {
                var files = checkpointDir.GetFiles();
                if (files.Length > 0)
                {
                    _fht.Recover();
                }
                foreach (var file in files)
                {
                    file.Delete();
                }
            }

            _session = NewSession();
        }

        public ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> NewSession(string sessionId = null, bool threadAffinitized = false)
        {
            return _fht.NewSession(new SimpleFunctions<Md5Key, DataValue>(), sessionId, threadAffinitized);
        }

        public ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> ResumeSession(string sessionId, out CommitPoint commitPoint, bool threadAffinitized = false)
        {
            return _fht.ResumeSession(new SimpleFunctions<Md5Key, DataValue>(), sessionId, out commitPoint, threadAffinitized);
        }

        public byte[] Get(string key)
        {
            return Get(key, _session);
        }

        public byte[] Get(string key, ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var (status, output) = session.Read(new Md5Key(key));

            return status == Status.OK ? output.Value : status == Status.PENDING && session.CompletePending(true) ? Get(key, session) : default;
        }

        public async ValueTask<byte[]> GetAsync(string key)
        {
            return await GetAsync(key, _session);
        }

        public async ValueTask<byte[]> GetAsync(string key, ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var result = await session.ReadAsync(new Md5Key(key));

            var (status, output) = result.Complete();

            return status == Status.OK ? output.Value : status == Status.PENDING && session.CompletePending(true) ? await GetAsync(key, session) : default;
        }

        public bool Set(string key, byte[] value)
        {
            return Set(key, value, _session);
        }

        public bool Set(string key, byte[] value, ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var status = session.Upsert(new Md5Key(key), new DataValue
            {
                Value = value
            });

            return status == Status.OK || (status == Status.PENDING && session.CompletePending(true));
        }

        public async Task<bool> SetAsync(string key, byte[] value)
        {
            return await SetAsync(key, value, _session);
        }

        public async Task<bool> SetAsync(string key, byte[] value, ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var result = await session.RMWAsync(new Md5Key(key), new DataValue
            {
                Value = value
            });

            (Status status, _) = result.Complete();

            return status == Status.OK || (status == Status.PENDING && session.CompletePending(true));
        }

        public bool Delete(string key)
        {
            return Delete(key, _session);
        }

        public bool Delete(string key, ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var status = session.Delete(new Md5Key(key));

            return status == Status.OK || (status == Status.PENDING && session.CompletePending(true));
        }

        public async Task<bool> DeleteAsync(string key)
        {
            return await DeleteAsync(key, _session);
        }

        public async Task<bool> DeleteAsync(string key, ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var result = await session.DeleteAsync(new Md5Key(key));

            var status = result.Complete();

            return status == Status.OK || (status == Status.PENDING && session.CompletePending(true));
        }

        public bool SaveSnapshot(bool saveHybridLog = true)
        {
            bool success = saveHybridLog
                ? _fht.TakeHybridLogCheckpoint(out _)
                : _fht.TakeFullCheckpoint(out _);

            _fht.CompleteCheckpointAsync().GetAwaiter().GetResult();

            return success;
        }

        public async ValueTask<bool> SaveSnapshotAsync(bool saveHybridLog = true)
        {
            (bool success, _) = saveHybridLog
                ? await _fht.TakeHybridLogCheckpointAsync(CheckpointType.Snapshot)
                : await _fht.TakeFullCheckpointAsync(CheckpointType.Snapshot);

            await _fht.CompleteCheckpointAsync();

            return success;
        }

        public void Dispose()
        {
            _fht.Dispose();
            _log.Dispose();
            _obj.Dispose();
        }
    }
}
