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
        private readonly string _path;
        private readonly IDevice _log;
        private readonly IDevice _obj;
        private readonly FasterKV<Md5Key, DataValue> _fht;
        private readonly ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> _session;

        /// <summary>
        /// Faster Key/Value in-memory and disk cache store
        /// </summary>
        /// <param name="path">the directory path of cache</param>
        /// <param name="size">size of hash table in #cache lines; 64 bytes per cache line</param>
        /// <param name="pageSizeBits">Size of a segment (group of pages), in bits: 9, 15, 22</param>
        /// <param name="memorySizeBits">Total size of in-memory part of log, in bits: 14, 20, 30</param>
        /// <param name="mutableFraction">Fraction of log marked as mutable (in-place updates): 0.1, 0.2, 0.3</param>
        public KVCache(string path = null, long size = 1L << 20, int pageSizeBits = 9, int memorySizeBits = 14, double mutableFraction = 0.1)
        {
            if (string.IsNullOrEmpty(path)) path = Path.GetTempPath();
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            _path = path;
            _log = Devices.CreateLogDevice(Path.Combine(path, "cache.log"));
            _obj = Devices.CreateLogDevice(Path.Combine(path, "cache.obj.log"));

            var checkpointDir = new DirectoryInfo(Path.Combine(path, "checkpoints"));
            var checkpointSettings = new CheckpointSettings
            {
                CheckpointDir = checkpointDir.FullName,
                CheckPointType = CheckpointType.Snapshot
            };
            var logSettings = new LogSettings
            {
                LogDevice = _log,
                ObjectLogDevice = _obj,
                PageSizeBits = pageSizeBits,
                MemorySizeBits = memorySizeBits,
                MutableFraction = mutableFraction
            };
            var serializerSettings = new SerializerSettings<Md5Key, DataValue>
            {
                keySerializer = () => new Md5KeySerializer(),
                valueSerializer = () => new DataValueSerializer()
            };

            _fht = new FasterKV<Md5Key, DataValue>(size, logSettings, checkpointSettings, serializerSettings);
            Recover(_fht, checkpointDir);
            _session = NewSession();
        }

        /// <summary>
        /// Auto recover using a separate index and log checkpoint token
        /// </summary>
        internal static void Recover<Key, Value>(IFasterKV<Key, Value> fht, DirectoryInfo checkpointDir)
        {
            if (!checkpointDir.Exists) return;
            var dirs = checkpointDir.GetDirectories();
            foreach (var dir in dirs)
            {
                if (!dir.Name.EndsWith("checkpoints")) continue;
                if (dir.Name.StartsWith("index"))
                {
                    dirs = dir.GetDirectories();
                    if (dirs.Length > 0 && Guid.TryParse(dirs[0].Name, out Guid fullCheckpointToken))
                    {
                        var files = dirs[0].GetFiles();
                        if (files.Length > 0 && files[0].Length > 64)
                        {
                            fht.Recover(fullCheckpointToken);
                        }
                    }
                    dir.FullName.DeleteDirectory();
                    break;
                }
                else
                {
                    dirs = dir.GetDirectories();
                    if (dirs.Length > 0 && Guid.TryParse(dirs[0].Name, out Guid hybridLogCheckpointToken))
                    {
                        var files = dirs[0].GetFiles();
                        if (files.Length > 0 && files[0].Length > 64)
                        {
                            fht.Recover(hybridLogCheckpointToken, hybridLogCheckpointToken);
                        }
                    }
                    dir.FullName.DeleteDirectory();
                    break;
                }
            }
        }

        /// <summary></summary>
        public FasterKV<Md5Key, DataValue> GetCache()
        {
            return _fht;
        }

        /// <summary></summary>
        public ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> GetSession()
        {
            return _session;
        }

        /// <summary></summary>
        public string GetDirectory()
        {
            return _path;
        }

        /// <summary>
        /// Start a new client session with FASTER. For performance reasons, 
        /// please use FasterKV<Key, Value>.For(functions).NewSession<Functions>(...) instead of this overload.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="threadAffinitized"></param>
        /// <returns></returns>
        public ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> NewSession(string sessionId = null, bool threadAffinitized = false)
        {
            return _fht.NewSession(new SimpleFunctions<Md5Key, DataValue>(), sessionId, threadAffinitized);
        }

        /// <summary>
        /// Resume (continue) prior client session with FASTER; used during recovery from
        /// failure. For performance reasons this overload is not recommended if functions
        /// is value type (struct).
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="commitPoint"></param>
        /// <param name="threadAffinitized"></param>
        /// <returns></returns>
        public ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> ResumeSession(string sessionId, out CommitPoint commitPoint, bool threadAffinitized = false)
        {
            return _fht.ResumeSession(new SimpleFunctions<Md5Key, DataValue>(), sessionId, out commitPoint, threadAffinitized);
        }

        /// <summary>
        /// Get operation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="wait">Wait for all pending operations on session to complete</param>
        /// <param name="spinWaitForCommit"></param>
        /// <returns></returns>
        public byte[] Get(string key, bool wait = false, bool spinWaitForCommit = false)
        {
            return Get(key, wait, spinWaitForCommit, _session);
        }

        /// <summary>
        /// Get operation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="wait">Wait for all pending operations on session to complete</param>
        /// <param name="spinWaitForCommit"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public byte[] Get(string key, bool wait, bool spinWaitForCommit, ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var (status, output) = session.Read(new Md5Key(key));

            return status == Status.OK ? output.Value : status == Status.PENDING && session.CompletePending(wait, spinWaitForCommit) ? Get(key, false, false, session) : default;
        }

        /// <summary>
        /// Get operation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="wait">Wait for all pending operations on session to complete</param>
        /// <param name="spinWaitForCommit"></param>
        /// <returns></returns>
        public async ValueTask<byte[]> GetAsync(string key, bool wait = false, bool spinWaitForCommit = false)
        {
            return await GetAsync(key, wait, spinWaitForCommit, _session);
        }

        /// <summary>
        /// Get operation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="wait">Wait for all pending operations on session to complete</param>
        /// <param name="spinWaitForCommit"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public async ValueTask<byte[]> GetAsync(string key, bool wait, bool spinWaitForCommit, ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var result = await session.ReadAsync(new Md5Key(key));

            var (status, output) = result.Complete();

            return status == Status.OK ? output.Value : status == Status.PENDING && session.CompletePending(wait, spinWaitForCommit) ? await GetAsync(key, false, false, session) : default;
        }

        /// <summary>
        /// Set operation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="wait">Wait for all pending operations on session to complete</param>
        /// <param name="spinWaitForCommit"></param>
        /// <returns></returns>
        public bool Set(string key, byte[] value, bool wait = false, bool spinWaitForCommit = false)
        {
            return Set(key, value, wait, spinWaitForCommit, _session);
        }

        /// <summary>
        /// Set operation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="wait">Wait for all pending operations on session to complete</param>
        /// <param name="spinWaitForCommit"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public bool Set(string key, byte[] value, bool wait, bool spinWaitForCommit, ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var status = session.Upsert(new Md5Key(key), new DataValue
            {
                Value = value
            });

            return status == Status.OK || (status == Status.PENDING && session.CompletePending(wait, spinWaitForCommit));
        }

        /// <summary>
        /// Set operation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="wait">Wait for all pending operations on session to complete</param>
        /// <param name="spinWaitForCommit"></param>
        /// <returns></returns>
        public async Task<bool> SetAsync(string key, byte[] value, bool wait = false, bool spinWaitForCommit = false)
        {
            return await SetAsync(key, value, wait, spinWaitForCommit, _session);
        }

        /// <summary>
        /// Set operation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="wait">Wait for all pending operations on session to complete</param>
        /// <param name="spinWaitForCommit"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public async Task<bool> SetAsync(string key, byte[] value, bool wait, bool spinWaitForCommit, ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var result = await session.RMWAsync(new Md5Key(key), new DataValue
            {
                Value = value
            });

            (Status status, _) = result.Complete();

            return status == Status.OK || (status == Status.PENDING && session.CompletePending(wait, spinWaitForCommit));
        }

        /// <summary>
        /// Delete operation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="wait">Wait for all pending operations on session to complete</param>
        /// <param name="spinWaitForCommit"></param>
        /// <returns></returns>
        public bool Delete(string key, bool wait = false, bool spinWaitForCommit = false)
        {
            return Delete(key, wait, spinWaitForCommit, _session);
        }

        /// <summary>
        /// Delete operation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="wait">Wait for all pending operations on session to complete</param>
        /// <param name="spinWaitForCommit"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public bool Delete(string key, bool wait, bool spinWaitForCommit, ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var status = session.Delete(new Md5Key(key));

            return status == Status.OK || (status == Status.PENDING && session.CompletePending(wait, spinWaitForCommit));
        }

        /// <summary>
        /// Delete operation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="wait">Wait for all pending operations on session to complete</param>
        /// <param name="spinWaitForCommit"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string key, bool wait = false, bool spinWaitForCommit = false)
        {
            return await DeleteAsync(key, wait, spinWaitForCommit, _session);
        }

        /// <summary>
        /// Delete operation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="wait">Wait for all pending operations on session to complete</param>
        /// <param name="spinWaitForCommit"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string key, bool wait, bool spinWaitForCommit, ClientSession<Md5Key, DataValue, DataValue, DataValue, Empty, IFunctions<Md5Key, DataValue, DataValue, DataValue, Empty>> session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var result = await session.DeleteAsync(new Md5Key(key));

            var status = result.Complete();

            return status == Status.OK || (status == Status.PENDING && session.CompletePending(wait, spinWaitForCommit));
        }

        /// <summary>
        /// Save snapshot and wait for ongoing full checkpoint to complete
        /// </summary>
        /// <param name="saveHybridLog">Save snapshot for hybrid log-only checkpoint</param>
        /// <returns></returns>
        public bool SaveSnapshot(bool saveHybridLog = true)
        {
            bool success = saveHybridLog
                ? _fht.TakeHybridLogCheckpoint(out _)
                : _fht.TakeFullCheckpoint(out _);

            _fht.CompleteCheckpointAsync().GetAwaiter().GetResult();

            return success;
        }

        /// <summary>
        /// Save snapshot and wait for ongoing full checkpoint to complete
        /// </summary>
        /// <param name="saveHybridLog">Save snapshot for hybrid log-only checkpoint</param>
        /// <returns></returns>
        public async ValueTask<bool> SaveSnapshotAsync(bool saveHybridLog = true)
        {
            (bool success, _) = saveHybridLog
                ? await _fht.TakeHybridLogCheckpointAsync(CheckpointType.Snapshot)
                : await _fht.TakeFullCheckpointAsync(CheckpointType.Snapshot);

            await _fht.CompleteCheckpointAsync();

            return success;
        }

        /// <summary></summary>
        public void Dispose()
        {
            _fht.Dispose();
            _log.Dispose();
            _obj.Dispose();
        }
    }
}
