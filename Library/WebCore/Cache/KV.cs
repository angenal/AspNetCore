using FASTER.core;
using System;
using System.IO;
using System.Threading;
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
        private readonly bool exists;
        private readonly string path;
        private readonly IDevice log;
        private readonly IDevice obj;
        private readonly FasterKV<TKey, TValue> fht;
        private readonly SimpleFunctions<TKey, TValue> fn = new SimpleFunctions<TKey, TValue>();

        /// <summary>
        /// Sets a new { keySerializer = () => new KeySerializer(), valueSerializer = () => new ValueSerializer() }
        /// </summary>
        public static SerializerSettings<TKey, TValue> SerializerSettings = null;

        /// <summary>
        /// Faster Key/Value in-memory cache
        /// </summary>
        /// <param name="size">size of hash table in #cache lines; 64 bytes per cache line</param>
        public KV(long size = 1L << 20)
        {
            this.size = size;
            log = new NullDevice();
            obj = new NullDevice();
            var logSettings = new LogSettings { LogDevice = log, ObjectLogDevice = obj };
            fht = new FasterKV<TKey, TValue>(size, logSettings, null, SerializerSettings);
        }

        /// <summary>
        /// Faster Key/Value in-memory and disk cache store
        /// </summary>
        /// <param name="path">Path to file that will store the log</param>
        /// <param name="size">hash table size (number of 64-byte buckets, each bucket is 64 bytes, 1 << 20 = 340M snapshot file)</param>
        /// <param name="pageSizeBits">Size of a segment (group of pages), in bits: 9, 15, 22</param>
        /// <param name="memorySizeBits">Total size of in-memory part of log, in bits: 14, 20, 30</param>
        /// <param name="mutableFraction">Fraction of log marked as mutable (in-place updates): 0.1, 0.2, 0.3</param>
        /// <param name="fullCheckpointToken">Initiate full checkpoint</param>
        /// <param name="seconds">Flush current log, Issue periodic checkpoints</param>
        public KV(string path, long size = 1L << 20, int pageSizeBits = 22, int memorySizeBits = 30, double mutableFraction = 0.1, Guid? fullCheckpointToken = null, int seconds = 0)
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
            var logPath = Path.Combine(path, $"{size}.log");
            exists = Directory.Exists(logPath);
            log = Devices.CreateLogDevice(logPath);
            obj = Devices.CreateLogDevice(Path.Combine(path, $"{size}.obj.log"));
            var checkpointSettings = new CheckpointSettings { CheckpointDir = path, CheckPointType = CheckpointType.Snapshot };
            var logSettings = new LogSettings { LogDevice = log, ObjectLogDevice = obj, PageSizeBits = pageSizeBits, MemorySizeBits = memorySizeBits, MutableFraction = mutableFraction };
            fht = new FasterKV<TKey, TValue>(size, logSettings, checkpointSettings, SerializerSettings);
            if (fullCheckpointToken.HasValue) fht.Recover(fullCheckpointToken.Value); else if (exists) fht.Recover();
            if (seconds > 1) IssuePeriodicCheckpoints(seconds * 1000);
        }

        private void IssuePeriodicCheckpoints(int milliseconds)
        {
            var t = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(milliseconds);
                    (_, _) = fht.TakeHybridLogCheckpointAsync(CheckpointType.FoldOver).GetAwaiter().GetResult();
                }
            })
            { IsBackground = true };
            t.Start();
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
            using (var s = fht.For(fn).NewSession<SimpleFunctions<TKey, TValue>>())
            {
                var status = s.Upsert(ref key, ref value);
                return status == Status.OK && s.CompletePending(wait, spinWaitForCommit);
            }
        }
        /// <summary>
        /// Set value
        /// </summary>
        public bool Set(TKey key, TValue value, string sessionId, bool resume = false, bool wait = false, bool spinWaitForCommit = false)
        {
            using (var s = resume == true && exists == true
                ? fht.For(fn).ResumeSession<SimpleFunctions<TKey, TValue>>(sessionId, out _)
                : fht.For(fn).NewSession<SimpleFunctions<TKey, TValue>>(sessionId))
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
            using (var s = fht.For(fn).NewSession<SimpleFunctions<TKey, TValue>>())
            {
                var valueOut = new TValue();
                var status = s.Read(ref key, ref valueOut);
                return status == Status.OK ? valueOut : default;
            }
        }
        /// <summary>
        /// Get value
        /// </summary>
        public TValue Get(TKey key, string sessionId, bool resume = false)
        {
            using (var s = resume == true && exists == true
                ? fht.For(fn).ResumeSession<SimpleFunctions<TKey, TValue>>(sessionId, out _)
                : fht.For(fn).NewSession<SimpleFunctions<TKey, TValue>>(sessionId))
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
            using (var s = fht.For(fn).NewSession<SimpleFunctions<TKey, TValue>>())
            {
                return (int)s.Delete(ref key);
            }
        }
        /// <summary>
        /// Delete value
        /// </summary>
        public int Delete(TKey key, string sessionId, bool resume = false)
        {
            using (var s = resume == true && exists == true
                ? fht.For(fn).ResumeSession<SimpleFunctions<TKey, TValue>>(sessionId, out _)
                : fht.For(fn).NewSession<SimpleFunctions<TKey, TValue>>(sessionId))
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
            using (var s = fht.For(fn).NewSession<SimpleFunctions<TKey, TValue>>())
            {
                return s.CompletePending(wait, spinWaitForCommit);
            }
        }

        /// <summary>
        /// Save snapshot and wait for ongoing full checkpoint to complete
        /// </summary>
        /// <param name="dispose"></param>
        /// <returns>Checkpoint token</returns>
        public async Task<Guid> SaveSnapshot(bool dispose = true)
        {
            Guid token = Guid.Empty;
            if (string.IsNullOrEmpty(path)) return token;
            while (!fht.TakeFullCheckpoint(out token)) CompletePending(true, true);
            await fht.CompleteCheckpointAsync();
            var filename = Path.Combine(path, $"{size}.checkpoint");
            File.WriteAllText(filename, token.ToString(), System.Text.Encoding.UTF8);
            if (!dispose) return token;
            fht.Dispose();
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
