using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using WebInterface;

namespace WebCore.Cache
{
    /// <summary>
    /// 内存+Redis缓存
    /// </summary>
    public class All : Cache
    {
        #region 属性
        /// <summary>内存缓存(字典缓存)</summary>
        protected ICache memory;
        /// <summary>Redis缓存</summary>
        protected ICache redis;
        #endregion

        #region 静态默认实现
        /// <summary>默认缓存</summary>
        public static ICache Instance { get; set; } = new All();
        #endregion

        #region 构造
        /// <summary>实例化一个内存字典缓存</summary>
        public All() : this(Memory.Instance, Redis.Instance)
        { }
        public All(ICache memory, ICache redis)
        {
            this.memory = memory;
            this.redis = redis;
        }
        #endregion

        #region 缓存属性
        /// <summary>缓存项。原子计数</summary>
        public override int Count => memory.Count;

        /// <summary>所有键。实际返回只读列表新实例，数据量较大时注意性能</summary>
        public override ICollection<string> Keys => memory.Keys;
        #endregion

        #region 基本操作
        /// <summary>是否包含缓存项</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override bool ContainsKey(string key) => memory.ContainsKey(key) || redis.ContainsKey(key);

        /// <summary>添加缓存项，已存在时更新</summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expire">过期时间，秒</param>
        /// <returns></returns>
        public override bool Set<T>(string key, T value, int expire = -1)
        {
            bool result = memory.Set<T>(key, value, expire);
            if (result)
            {
                JobManager.AddJob(new Action(() => redis.Set<T>(key, value, expire)), s => s.ToRunNow());
            }
            return result;
        }

        /// <summary>获取缓存项，不存在时返回默认值</summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public override T Get<T>(string key)
        {
            T item = memory.Get<T>(key);
            if (item == null)
            {
                item = redis.Get<T>(key);
                if (item == null) return item;
                int expire = (int)redis.GetExpire(key).TotalSeconds;
                if (expire > 2) memory.Set<T>(key, item, expire);
            }
            return item;
        }

        /// <summary>批量移除缓存项</summary>
        /// <param name="keys">键集合</param>
        /// <returns>实际移除个数</returns>
        public override int Remove(params string[] keys)
        {
            var count = memory.Remove(keys);
            if (count > 0)
            {
                redis.Remove(keys);
            }
            return count;
        }

        /// <summary>清空所有缓存项</summary>
        public override void Clear()
        {
            memory.Clear();
            redis.Clear();
        }

        /// <summary>设置缓存项有效期</summary>
        /// <param name="key">键</param>
        /// <param name="expire">过期时间</param>
        /// <returns>设置是否成功</returns>
        public override bool SetExpire(string key, TimeSpan expire)
        {
            bool result = memory.SetExpire(key, expire);
            if (result)
            {
                result = redis.SetExpire(key, expire);
            }
            return result;
        }

        /// <summary>获取缓存项有效期，不存在时返回Zero</summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public override TimeSpan GetExpire(string key)
        {
            TimeSpan ts = memory.GetExpire(key);
            if (ts == TimeSpan.Zero)
            {
                ts = redis.GetExpire(key);
            }
            return ts;
        }
        #endregion

        #region 高级操作
        /// <summary>添加，已存在时不更新，常用于锁争夺</summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expire">过期时间，秒</param>
        /// <returns></returns>
        public override bool Add<T>(string key, T value, int expire = -1)
        {
            bool result = memory.Add<T>(key, value, expire);
            if (result)
            {
                JobManager.AddJob(new Action(() => redis.Add<T>(key, value, expire)), s => s.ToRunNow());
            }
            return result;
        }

        /// <summary>设置新值并获取旧值，原子操作</summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public override T Replace<T>(string key, T value)
        {
            T result = memory.Replace<T>(key, value);
            JobManager.AddJob(new Action(() => redis.Replace<T>(key, value)), s => s.ToRunNow());
            return result;
        }

        /// <summary>尝试获取指定键，返回是否包含值。有可能缓存项刚好是默认值，或者只是反序列化失败</summary>
        /// <remarks>
        /// 在 MemoryCache 中，如果某个key过期，在清理之前仍然可以通过TryGet访问，并且更新访问时间，避免被清理。
        /// </remarks>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值。即使有值也不一定能够返回，可能缓存项刚好是默认值，或者只是反序列化失败</param>
        /// <returns>返回是否包含值，即使反序列化失败</returns>
        public override bool TryGetValue<T>(string key, out T value)
        {
            bool result = memory.TryGetValue<T>(key, out value);
            if (!result)
            {
                result = redis.TryGetValue<T>(key, out value);
            }
            return result;
        }

        /// <summary>获取 或 添加 缓存数据，在数据不存在时执行委托请求数据</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        /// <param name="expire">过期时间，秒。小于0时采用默认缓存时间<seealso cref="Cache.Expire"/></param>
        /// <returns></returns>
        public override T GetOrAdd<T>(string key, Func<string, T> callback, int expire = -1)
        {
            T result = memory.GetOrAdd<T>(key, callback, expire);
            JobManager.AddJob(new Action(() => redis.GetOrAdd<T>(key, callback, expire)), s => s.ToRunNow());
            return result;
        }

        /// <summary>累加，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public override long Increment(string key, long value)
        {
            long result = memory.Increment(key, value);
            JobManager.AddJob(new Action(() => redis.Increment(key, value)), s => s.ToRunNow());
            return result;
        }

        /// <summary>累加，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public override double Increment(string key, double value)
        {
            double result = memory.Increment(key, value);
            JobManager.AddJob(new Action(() => redis.Increment(key, value)), s => s.ToRunNow());
            return result;
        }

        /// <summary>递减，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public override long Decrement(string key, long value)
        {
            long result = memory.Decrement(key, value);
            JobManager.AddJob(new Action(() => redis.Decrement(key, value)), s => s.ToRunNow());
            return result;
        }

        /// <summary>递减，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public override double Decrement(string key, double value)
        {
            double result = memory.Decrement(key, value);
            JobManager.AddJob(new Action(() => redis.Decrement(key, value)), s => s.ToRunNow());
            return result;
        }
        #endregion

        #region 集合操作
        /// <summary>设置列表</summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="values">列表值</param>
        /// <param name="expire">过期时间，秒。小于0时采用默认缓存时间<seealso cref="Cache.Expire"/></param>
        public override void SetList<T>(string key, IList<T> values, int expire = -1)
        {
            memory.SetList<T>(key, values, expire);
            JobManager.AddJob(new Action(() => redis.SetList<T>(key, values, expire)), s => s.ToRunNow());
        }

        /// <summary>获取列表</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public override IList<T> GetList<T>(string key)
        {
            IList<T> result = memory.GetList<T>(key);
            if (!result.Any())
            {
                result = redis.GetList<T>(key);
                if (!result.Any()) return result;
                int expire = (int)redis.GetExpire(key).TotalSeconds;
                if (expire > 2) memory.SetList<T>(key, result, expire);
            }
            return result;
        }

        /// <summary>获取哈希</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public override IDictionary<string, T> GetDictionary<T>(string key)
        {
            IDictionary<string, T> result = memory.GetDictionary<T>(key);
            return result;
        }

        /// <summary>获取队列</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public override IProducerConsumer<T> GetQueue<T>(string key)
        {
            IProducerConsumer<T> result = memory.GetQueue<T>(key);
            return result;
        }

        /// <summary>获取栈</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public override IProducerConsumer<T> GetStack<T>(string key)
        {
            IProducerConsumer<T> result = memory.GetStack<T>(key);
            return result;
        }

        /// <summary>获取Set</summary>
        /// <remarks>基于HashSet，非线程安全</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public override ICollection<T> GetSet<T>(string key)
        {
            ICollection<T> result = memory.GetSet<T>(key);
            return result;
        }
        #endregion
    }
}
