using System;
using System.Collections.Generic;
using WebInterface;

namespace WebCore.Cache
{
    /// <summary>缓存</summary>
    public abstract class Cache : ICache
    {
        /// <summary>默认缓存</summary>
        public static ICache Default { get; set; } = new Memory();

        #region 属性
        /// <summary>名称</summary>
        public string Name { get; set; }

        /// <summary>默认过期时间(1天)。避免Set操作时没有设置过期时间，默认0秒表示不过期</summary>
        public int Expire { get; set; } = 24 * 3600;

        /// <summary>获取和设置缓存，使用默认过期时间</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual object this[string key] { get => Get<object>(key); set => Set(key, value); }

        /// <summary>缓存个数</summary>
        public abstract int Count { get; }

        /// <summary>所有键</summary>
        public abstract ICollection<string> Keys { get; }
        #endregion

        #region 构造
        /// <summary>构造函数</summary>
        protected Cache() { }
        #endregion

        #region 基础操作
        /// <summary>使用连接字符串初始化配置</summary>
        /// <param name="config"></param>
        public virtual void Init(string config) { }

        /// <summary>是否包含缓存项</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract bool ContainsKey(string key);

        /// <summary>设置缓存项</summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expire">过期时间，秒</param>
        /// <returns></returns>
        public abstract bool Set<T>(string key, T value, int expire = -1);

        /// <summary>设置缓存项</summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expire">过期时间</param>
        /// <returns></returns>
        public virtual bool Set<T>(string key, T value, TimeSpan expire) => Set(key, value, (int)expire.TotalSeconds);

        /// <summary>获取缓存项</summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public abstract T Get<T>(string key);

        /// <summary>批量移除缓存项</summary>
        /// <param name="keys">键集合</param>
        /// <returns></returns>
        public abstract int Remove(params string[] keys);

        /// <summary>清空所有缓存项</summary>
        public virtual void Clear() => throw new NotSupportedException();

        /// <summary>设置缓存项有效期</summary>
        /// <param name="key">键</param>
        /// <param name="expire">过期时间，秒</param>
        public abstract bool SetExpire(string key, TimeSpan expire);

        /// <summary>获取缓存项有效期</summary>
        /// <param name="key">键</param>
        /// <returns>永不过期：TimeSpan.MaxValue 不存在该缓存：TimeSpan.Zero</returns>
        public abstract TimeSpan GetExpire(string key);
        #endregion

        #region 集合操作
        /// <summary>批量获取缓存项</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            var dic = new Dictionary<string, T>();
            foreach (var key in keys) dic[key] = Get<T>(key);
            return dic;
        }

        /// <summary>批量设置缓存项</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="expire">过期时间，秒</param>
        public virtual void SetAll<T>(IDictionary<string, T> values, int expire = -1)
        {
            foreach (var item in values) Set(item.Key, item.Value, expire);
        }

        /// <summary>设置列表</summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="values">列表值</param>
        /// <param name="expire">过期时间，秒。小于0时采用默认缓存时间<seealso cref="Expire"/></param>
        public virtual void SetList<T>(string key, IList<T> values, int expire = -1) => throw new NotSupportedException();

        /// <summary>获取列表</summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public virtual IList<T> GetList<T>(string key) => throw new NotSupportedException();

        /// <summary>获取哈希</summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public virtual IDictionary<string, T> GetDictionary<T>(string key) => throw new NotSupportedException();

        /// <summary>获取队列</summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public virtual IProducerConsumer<T> GetQueue<T>(string key) => throw new NotSupportedException();

        /// <summary>获取栈</summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public virtual IProducerConsumer<T> GetStack<T>(string key) => throw new NotSupportedException();

        /// <summary>获取Set</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual ICollection<T> GetSet<T>(string key) => throw new NotSupportedException();
        #endregion

        #region 高级操作
        /// <summary>添加，已存在时不更新，常用于锁争夺</summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expire">过期时间，秒。小于0时采用默认缓存时间<seealso cref="Cache.Expire"/></param>
        /// <returns></returns>
        public virtual bool Add<T>(string key, T value, int expire = -1)
        {
            if (ContainsKey(key)) return false;
            return Set(key, value, expire);
        }

        /// <summary>设置新值并获取旧值，原子操作</summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public virtual T Replace<T>(string key, T value)
        {
            var rs = Get<T>(key);
            Set(key, value);
            return rs;
        }

        /// <summary>尝试获取指定键，返回是否包含值。有可能缓存项刚好是默认值，或者只是反序列化失败</summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值。即使有值也不一定能够返回，可能缓存项刚好是默认值，或者只是反序列化失败</param>
        /// <returns>返回是否包含值，即使反序列化失败</returns>
        public virtual bool TryGetValue<T>(string key, out T value)
        {
            value = Get<T>(key);
            if (!Equals(value, default)) return true;
            return ContainsKey(key);
        }

        /// <summary>获取 或 添加 缓存数据，在数据不存在时执行委托请求数据</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        /// <param name="expire">过期时间，秒。小于0时采用默认缓存时间<seealso cref="Cache.Expire"/></param>
        /// <returns></returns>
        public virtual T GetOrAdd<T>(string key, Func<string, T> callback, int expire = -1)
        {
            var value = Get<T>(key);
            if (!Equals(value, default)) return value;
            if (ContainsKey(key)) return value;
            value = callback(key);
            if (expire < 0) expire = Expire;
            if (Add(key, value, expire)) return value;
            return Get<T>(key);
        }

        /// <summary>累加，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public virtual long Increment(string key, long value)
        {
            lock (this)
            {
                var v = Get<long>(key);
                v += value;
                Set(key, v);
                return v;
            }
        }

        /// <summary>累加，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public virtual double Increment(string key, double value)
        {
            lock (this)
            {
                var v = Get<double>(key);
                v += value;
                Set(key, v);
                return v;
            }
        }

        /// <summary>递减，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public virtual long Decrement(string key, long value)
        {
            lock (this)
            {
                var v = Get<long>(key);
                v -= value;
                Set(key, v);
                return v;
            }
        }

        /// <summary>递减，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public virtual double Decrement(string key, double value)
        {
            lock (this)
            {
                var v = Get<double>(key);
                v -= value;
                Set(key, v);
                return v;
            }
        }
        #endregion
    }
}
