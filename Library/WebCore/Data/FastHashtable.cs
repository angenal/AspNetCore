using FASTER.core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebCore.Data
{
    /// <summary>
    /// Faster Hashtable
    /// </summary>
    /// <typeparam name="TKey">Recommend string</typeparam>
    /// <typeparam name="TValue">Custom class</typeparam>
    public class FastHashtable<TKey, TValue> where TValue : new()
    {
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
        public FastHashtable(string path, long sizeBytes = 1 << 20, int pageSizeBits = 22, int memorySizeBits = 30, double mutableFraction = 0.1, Guid? fullCheckpointToken = null)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (!fullCheckpointToken.HasValue)
            {
                var filename = Path.Combine(path, $"{nameof(TValue)}.checkpoint");
                if (File.Exists(filename))
                {
                    var s = File.ReadAllText(filename, System.Text.Encoding.UTF8);
                    if (Guid.TryParse(s, out Guid guid)) fullCheckpointToken = guid;
                }
            }
            this.path = path;
            log = Devices.CreateLogDevice(Path.Combine(path, $"{nameof(TValue)}.log"));
            obj = Devices.CreateLogDevice(Path.Combine(path, $"{nameof(TValue)}.cache"));
            var checkpointSettings = new CheckpointSettings { CheckpointDir = path, CheckPointType = CheckpointType.Snapshot };
            var logSettings = new LogSettings { LogDevice = log, ObjectLogDevice = obj, PageSizeBits = pageSizeBits, MemorySizeBits = memorySizeBits, MutableFraction = mutableFraction };
            fht = new FasterKV<TKey, TValue>(sizeBytes, logSettings, checkpointSettings, SerializerSettings);
            if (fullCheckpointToken.HasValue) fht.Recover(fullCheckpointToken.Value);
            else if (File.Exists(log.FileName)) fht.Recover();
        }

        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="wait"></param>
        /// <param name="spinWaitForCommit"></param>
        /// <returns></returns>
        public bool SetValue(TKey key, TValue value, bool wait = false, bool spinWaitForCommit = false)
        {
            using (var s = fht.For(fn).NewSession<SimpleFunctions<TKey, TValue>>())
            {
                var i = (int)s.Upsert(ref key, ref value);
                return s.CompletePending(wait, spinWaitForCommit);
            }
        }

        /// <summary>
        /// Get value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue GetValue(TKey key)
        {
            using (var s = fht.For(fn).NewSession<SimpleFunctions<TKey, TValue>>())
            {
                var valueOut = new TValue();
                var status = s.Read(ref key, ref valueOut);
                return valueOut;
            }
        }

        /// <summary>
        /// Delete value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int Delete(TKey key)
        {
            using (var s = fht.For(fn).NewSession<SimpleFunctions<TKey, TValue>>())
            {
                return (int)s.Delete(ref key);
            }
        }

        /// <summary>
        /// Wait for commit of all operations completed until the current point in session.
        /// </summary>
        public void CompletePending(bool wait = true, bool spinWaitForCommit = true)
        {
            using (var s = fht.For(fn).NewSession<SimpleFunctions<TKey, TValue>>())
            {
                s.CompletePending(wait, spinWaitForCommit);
            }
        }

        /// <summary>
        /// Hashtable Dispose
        /// </summary>
        /// <returns></returns>
        public async Task<Guid> Dispose()
        {
            fht.TakeFullCheckpoint(out Guid token);
            await fht.CompleteCheckpointAsync();
            var filename = Path.Combine(path, $"{nameof(TValue)}.checkpoint");
            File.WriteAllText(filename, token.ToString(), System.Text.Encoding.UTF8);
            fht.Dispose();
            log.Dispose();
            obj.Dispose();
            return token;
        }
    }
}
