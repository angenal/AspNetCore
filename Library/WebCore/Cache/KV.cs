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
    /// <typeparam name="TKey">Recommend string</typeparam>
    /// <typeparam name="TValue">Custom class</typeparam>
    public class KV<TKey, TValue> where TValue : new()
    {
        private readonly long size;
        private readonly string path;
        private readonly IDevice log;
        private readonly IDevice obj;
        private readonly FasterKV<TKey, TValue> store;
        private readonly SimpleFunctions<TKey, TValue> fn = new SimpleFunctions<TKey, TValue>();

        /// <summary>
        /// Sets a new { keySerializer = () => new KeySerializer(), valueSerializer = () => new ValueSerializer() }
        /// </summary>
        public static SerializerSettings<TKey, TValue> SerializerSettings = null;

        /// <summary>
        /// Faster Key/Value in-memory cache
        /// https://microsoft.github.io/FASTER
        /// </summary>
        /// <param name="size"></param>
        public KV(long size = 1L << 20)
        {
            this.size = size;
            log = new NullDevice();
            obj = new NullDevice();
            var logSettings = new LogSettings { LogDevice = log, ObjectLogDevice = obj };
            store = new FasterKV<TKey, TValue>(size, logSettings, null, SerializerSettings);
        }

        /// <summary>
        /// Faster Key/Value cache store
        /// https://microsoft.github.io/FASTER
        /// </summary>
        /// <param name="path">Path to file that will store the log</param>
        /// <param name="size">hash table size (number of 64-byte buckets, each bucket is 64 bytes, 1 << 20 = 340M snapshot file)</param>
        /// <param name="pageSizeBits">Size of a segment (group of pages), in bits</param>
        /// <param name="memorySizeBits">Total size of in-memory part of log, in bits</param>
        /// <param name="mutableFraction">Fraction of log marked as mutable (in-place updates)</param>
        public KV(string path, long size = 1L << 20, int pageSizeBits = 22, int memorySizeBits = 30, double mutableFraction = 0.1, Guid? fullCheckpointToken = null)
        {
            this.size = size;
            var dirname = typeof(TValue).Name;
            if (!Path.GetDirectoryName(path).EndsWith(dirname)) path = Path.Combine(path, dirname);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (!fullCheckpointToken.HasValue)
            {
                var filename = Path.Combine(path, $"{size}.checkpoint");
                if (File.Exists(filename))
                {
                    var s = File.ReadAllText(filename, System.Text.Encoding.UTF8);
                    if (Guid.TryParse(s, out Guid guid)) fullCheckpointToken = guid;
                }
            }
            this.path = path;
            log = Devices.CreateLogDevice(Path.Combine(path, $"{size}.log"));
            obj = Devices.CreateLogDevice(Path.Combine(path, $"{size}.obj.log"));
            var checkpointSettings = new CheckpointSettings { CheckpointDir = path, CheckPointType = CheckpointType.Snapshot };
            var logSettings = new LogSettings { LogDevice = log, ObjectLogDevice = obj, PageSizeBits = pageSizeBits, MemorySizeBits = memorySizeBits, MutableFraction = mutableFraction };
            store = new FasterKV<TKey, TValue>(size, logSettings, checkpointSettings, SerializerSettings);
            if (fullCheckpointToken.HasValue) store.Recover(fullCheckpointToken.Value);
            else if (File.Exists(log.FileName)) store.Recover();
        }

        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="wait">Wait for all pending operations on session to complete</param>
        /// <param name="spinWaitForCommit">Spin-wait until ongoing commit/checkpoint, if any, completes</param>
        /// <returns>True if update and pending operation have completed, false otherwise</returns>
        public bool Set(TKey key, TValue value, bool wait = false, bool spinWaitForCommit = false)
        {
            using (var s = store.For(fn).NewSession<SimpleFunctions<TKey, TValue>>())
            {
                var status = s.Upsert(ref key, ref value);
                return status == Status.OK && s.CompletePending(wait, spinWaitForCommit);
            }
        }

        /// <summary>
        /// Get value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue Get(TKey key)
        {
            using (var s = store.For(fn).NewSession<SimpleFunctions<TKey, TValue>>())
            {
                var valueOut = new TValue();
                var status = s.Read(ref key, ref valueOut);
                return status == Status.OK ? valueOut : default;
            }
        }

        /// <summary>
        /// Delete value
        /// </summary>
        /// <param name="key"></param>
        /// <returns>OK = 0, NOTFOUND = 1, PENDING = 2, ERROR = 3</returns>
        public int Delete(TKey key)
        {
            using (var s = store.For(fn).NewSession<SimpleFunctions<TKey, TValue>>())
            {
                return (int)s.Delete(ref key);
            }
        }

        /// <summary>
        /// Complete outstanding pending operations
        /// </summary>
        /// <param name="wait">Wait for all pending operations on session to complete</param>
        /// <param name="spinWaitForCommit">Spin-wait until ongoing commit/checkpoint, if any, completes</param>
        /// <returns>True if all pending operations have completed, false otherwise</returns>
        public bool CompletePending(bool wait = false, bool spinWaitForCommit = false)
        {
            using (var s = store.For(fn).NewSession<SimpleFunctions<TKey, TValue>>())
            {
                return s.CompletePending(wait, spinWaitForCommit);
            }
        }

        /// <summary>
        /// Save snapshot and wait for ongoing checkpoint to complete
        /// </summary>
        /// <param name="dispose"></param>
        /// <returns>Checkpoint token</returns>
        public async Task<Guid> SaveSnapshot(bool dispose = true)
        {
            if (string.IsNullOrEmpty(path)) return Guid.Empty;
            if (!store.TakeFullCheckpoint(out Guid token)) CompletePending(true, true);
            await store.CompleteCheckpointAsync();
            var filename = Path.Combine(path, $"{size}.checkpoint");
            File.WriteAllText(filename, token.ToString(), System.Text.Encoding.UTF8);
            if (!dispose) return token;
            store.Dispose();
            log.Dispose();
            obj.Dispose();
            return token;
        }

        /// <summary>
        /// Hashtable dispose and wait for ongoing checkpoint to complete
        /// </summary>
        /// <returns>Checkpoint token</returns>
        public async Task<Guid> Dispose()
        {
            return await SaveSnapshot(true);
        }
    }
}
