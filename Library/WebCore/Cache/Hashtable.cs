using FASTER.core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebCore.Cache
{
    /// <summary>
    /// Faster Hashtable
    /// https://github.com/microsoft/FASTER
    /// </summary>
    /// <typeparam name="TKey">Recommend string</typeparam>
    /// <typeparam name="TValue">Custom class</typeparam>
    public class Hashtable<TKey, TValue> where TValue : new()
    {
        private readonly long size;
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
        /// Faster Hashtable
        /// </summary>
        /// <param name="path">Path to file that will store the log</param>
        /// <param name="sizeBytes">Size of index in #cache lines (64 bytes each) 1 << 20 = 340M snapshot file</param>
        /// <param name="pageSizeBits">Size of a segment (group of pages), in bits</param>
        /// <param name="memorySizeBits">Total size of in-memory part of log, in bits</param>
        /// <param name="mutableFraction">Fraction of log marked as mutable (in-place updates)</param>
        public Hashtable(string path, long sizeBytes = 1 << 20, int pageSizeBits = 22, int memorySizeBits = 30, double mutableFraction = 0.1, Guid? fullCheckpointToken = null)
        {
            size = sizeBytes;
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
            obj = Devices.CreateLogDevice(Path.Combine(path, $"{size}.cache"));
            var checkpointSettings = new CheckpointSettings { CheckpointDir = path, CheckPointType = CheckpointType.Snapshot };
            var logSettings = new LogSettings { LogDevice = log, ObjectLogDevice = obj, PageSizeBits = pageSizeBits, MemorySizeBits = memorySizeBits, MutableFraction = mutableFraction };
            fht = new FasterKV<TKey, TValue>(size, logSettings, checkpointSettings, SerializerSettings);
            if (fullCheckpointToken.HasValue) fht.Recover(fullCheckpointToken.Value);
            else if (File.Exists(log.FileName)) fht.Recover();
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
        /// Hashtable dispose and wait for ongoing checkpoint to complete
        /// </summary>
        /// <returns>Checkpoint token</returns>
        public async Task<Guid> Dispose()
        {
            fht.TakeFullCheckpoint(out Guid token);
            await fht.CompleteCheckpointAsync();
            var filename = Path.Combine(path, $"{size}.checkpoint");
            File.WriteAllText(filename, token.ToString(), System.Text.Encoding.UTF8);
            fht.Dispose();
            log.Dispose();
            obj.Dispose();
            return token;
        }
    }
}
