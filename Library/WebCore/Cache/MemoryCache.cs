using FluentScheduler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebInterface;

namespace WebCore.Cache
{
    /// <summary>
    /// 内存缓存(字典缓存)
    /// </summary>
    public class MemoryCache : Cache
    {
        #region 属性
        /// <summary>缓存核心</summary>
        protected ConcurrentDictionary<string, CacheItem> _cache;

        /// <summary>容量。容量超标时，采用LRU机制删除，默认1000000</summary>
        public int Capacity { get; set; } = 1000000;

        /// <summary>定时清理时间，默认10秒</summary>
        public int Period { get; set; } = 10;

        /// <summary>缓存键过期</summary>
        public event EventHandler<EventArgs<string>> KeyExpired;
        #endregion

        #region 静态默认实现
        /// <summary>默认缓存</summary>
        public static ICache Instance { get; set; } = new MemoryCache();
        #endregion

        #region 构造
        /// <summary>实例化一个内存字典缓存</summary>
        public MemoryCache()
        {
            Init(null);
        }
        #endregion

        #region 缓存属性
        private int _count;
        /// <summary>缓存项。原子计数</summary>
        public override int Count => _count;

        /// <summary>所有键。实际返回只读列表新实例，数据量较大时注意性能</summary>
        public override ICollection<string> Keys => _cache.Keys;
        #endregion

        #region 方法
        /// <summary>初始化配置</summary>
        /// <param name="config"></param>
        public override void Init(string config)
        {
            if (_cache == null)
            {
                _cache = new ConcurrentDictionary<string, CacheItem>();
                int interval = Period < 10 ? 10 : Period;
                JobManager.AddJob(RemoveNotAlive, s => s.ToRunEvery(interval));
            }
        }

        /// <summary>获取或添加缓存项</summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expire">过期时间，秒</param>
        /// <returns></returns>
        public virtual T GetOrAdd<T>(string key, T value, int expire = -1)
        {
            if (expire < 0) expire = Expire;

            CacheItem ci = null;
            do
            {
                if (_cache.TryGetValue(key, out var item)) return (T)item.Visit();

                if (ci == null) ci = new CacheItem(value, expire);
            } while (!_cache.TryAdd(key, ci));

            Interlocked.Increment(ref _count);

            return (T)ci.Visit();
        }
        #endregion

        #region 基本操作
        /// <summary>是否包含缓存项</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override bool ContainsKey(string key) => _cache.TryGetValue(key, out var item) && item != null && !item.Expired;

        /// <summary>添加缓存项，已存在时更新</summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expire">过期时间，秒</param>
        /// <returns></returns>
        public override bool Set<T>(string key, T value, int expire = -1)
        {
            if (expire < 0) expire = Expire;

            //_cache.AddOrUpdate(key,
            //    k => new CacheItem(value, expire),
            //    (k, item) =>
            //    {
            //        item.Value = value;
            //        item.ExpiredTime = DateTime.Now.AddSeconds(expire);

            //        return item;
            //    });

            // 不用AddOrUpdate，避免匿名委托带来的GC损耗
            CacheItem ci = null;
            do
            {
                if (_cache.TryGetValue(key, out var item))
                {
                    item.Set(value, expire);
                    return true;
                }

                if (ci == null) ci = new CacheItem(value, expire);
            } while (!_cache.TryAdd(key, ci));

            Interlocked.Increment(ref _count);

            return true;
        }

        /// <summary>获取缓存项，不存在时返回默认值</summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public override T Get<T>(string key)
        {
            if (!_cache.TryGetValue(key, out var item) || item == null || item.Expired) return default;

            return item.Visit().As<T>();
        }

        /// <summary>批量移除缓存项</summary>
        /// <param name="keys">键集合</param>
        /// <returns>实际移除个数</returns>
        public override int Remove(params string[] keys)
        {
            var count = 0;
            foreach (var k in keys)
            {
                if (_cache.TryRemove(k, out _))
                {
                    count++;

                    Interlocked.Decrement(ref _count);
                }
            }
            return count;
        }

        /// <summary>清空所有缓存项</summary>
        public override void Clear()
        {
            _cache.Clear();
            _count = 0;
        }

        /// <summary>设置缓存项有效期</summary>
        /// <param name="key">键</param>
        /// <param name="expire">过期时间</param>
        /// <returns>设置是否成功</returns>
        public override bool SetExpire(string key, TimeSpan expire)
        {
            if (!_cache.TryGetValue(key, out var item) || item == null) return false;

            item.ExpiredTime = DateTime.Now.Add(expire);

            return true;
        }

        /// <summary>获取缓存项有效期，不存在时返回Zero</summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public override TimeSpan GetExpire(string key)
        {
            if (!_cache.TryGetValue(key, out var item) || item == null) return TimeSpan.Zero;

            return item.ExpiredTime - DateTime.Now;
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
            if (expire < 0) expire = Expire;

            CacheItem ci = null;
            do
            {
                if (_cache.TryGetValue(key, out _)) return false;

                if (ci == null) ci = new CacheItem(value, expire);
            } while (!_cache.TryAdd(key, ci));

            Interlocked.Increment(ref _count);

            return true;
        }

        /// <summary>设置新值并获取旧值，原子操作</summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public override T Replace<T>(string key, T value)
        {
            var expire = Expire;

            CacheItem ci = null;
            do
            {
                if (_cache.TryGetValue(key, out var item))
                {
                    var rs = item.Value;
                    // 如果已经过期，不要返回旧值
                    if (item.Expired) rs = default(T);
                    item.Set(value, expire);
                    return (T)rs;
                }

                if (ci == null) ci = new CacheItem(value, expire);
            } while (!_cache.TryAdd(key, ci));

            Interlocked.Increment(ref _count);

            return default;
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
            value = default;

            // 没有值，直接结束
            if (!_cache.TryGetValue(key, out var item) || item == null) return false;

            // 得到已有值
            value = item.Visit().As<T>();

            // 是否未过期的有效值
            return !item.Expired;
        }

        /// <summary>获取 或 添加 缓存数据，在数据不存在时执行委托请求数据</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        /// <param name="expire">过期时间，秒。小于0时采用默认缓存时间<seealso cref="Cache.Expire"/></param>
        /// <returns></returns>
        public override T GetOrAdd<T>(string key, Func<string, T> callback, int expire = -1)
        {
            if (expire < 0) expire = Expire;

            CacheItem ci = null;
            do
            {
                if (_cache.TryGetValue(key, out var item)) return (T)item.Visit();

                if (ci == null) ci = new CacheItem(callback(key), expire);
            } while (!_cache.TryAdd(key, ci));

            Interlocked.Increment(ref _count);

            return (T)ci.Visit();
        }

        /// <summary>累加，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public override long Increment(string key, long value)
        {
            var item = GetOrAddItem(key, k => 0L);
            return (long)item.Inc(value);
        }

        /// <summary>累加，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public override double Increment(string key, double value)
        {
            var item = GetOrAddItem(key, k => 0d);
            return (double)item.Inc(value);
        }

        /// <summary>递减，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public override long Decrement(string key, long value)
        {
            var item = GetOrAddItem(key, k => 0L);
            return (long)item.Dec(value);
        }

        /// <summary>递减，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public override double Decrement(string key, double value)
        {
            var item = GetOrAddItem(key, k => 0d);
            return (double)item.Dec(value);
        }
        #endregion

        #region 集合操作
        /// <summary>获取列表</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public override IList<T> GetList<T>(string key)
        {
            var item = GetOrAddItem(key, k => new List<T>());
            return item.Visit() as IList<T>;
        }

        /// <summary>获取哈希</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public override IDictionary<string, T> GetDictionary<T>(string key)
        {
            var item = GetOrAddItem(key, k => new ConcurrentDictionary<string, T>());
            return item.Visit() as IDictionary<string, T>;
        }

        /// <summary>获取队列</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public override IProducerConsumer<T> GetQueue<T>(string key)
        {
            var item = GetOrAddItem(key, k => new MemoryQueue<T>());
            return item.Visit() as IProducerConsumer<T>;
        }

        /// <summary>获取栈</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public override IProducerConsumer<T> GetStack<T>(string key)
        {
            var item = GetOrAddItem(key, k => new MemoryQueue<T>(new ConcurrentStack<T>()));
            return item.Visit() as IProducerConsumer<T>;
        }

        /// <summary>获取Set</summary>
        /// <remarks>基于HashSet，非线程安全</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public override ICollection<T> GetSet<T>(string key)
        {
            var item = GetOrAddItem(key, k => new HashSet<T>());
            return item.Visit() as ICollection<T>;
        }

        /// <summary>获取 或 添加 缓存项</summary>
        /// <param name="key"></param>
        /// <param name="valueFactory"></param>
        /// <returns></returns>
        protected CacheItem GetOrAddItem(string key, Func<string, object> valueFactory)
        {
            var expire = Expire;

            CacheItem ci = null;
            do
            {
                if (_cache.TryGetValue(key, out var item)) return item;

                if (ci == null) ci = new CacheItem(valueFactory(key), expire);
            } while (!_cache.TryAdd(key, ci));

            Interlocked.Increment(ref _count);

            return ci;
        }
        #endregion

        #region 缓存项
        /// <summary>缓存项</summary>
        protected class CacheItem
        {
            private object _Value;
            /// <summary>数值</summary>
            public object Value { get => _Value; set => _Value = value; }

            /// <summary>过期时间</summary>
            public DateTime ExpiredTime { get; set; }

            /// <summary>是否过期</summary>
            public bool Expired => ExpiredTime <= DateTime.Now;

            /// <summary>访问时间</summary>
            public DateTime VisitTime { get; private set; }

            /// <summary>构造缓存项</summary>
            /// <param name="value"></param>
            /// <param name="expire"></param>
            public CacheItem(object value, int expire) => Set(value, expire);

            /// <summary>设置数值和过期时间</summary>
            /// <param name="value"></param>
            /// <param name="expire"></param>
            public void Set(object value, int expire)
            {
                Value = value;

                var now = VisitTime = DateTime.Now;
                if (expire <= 0)
                    ExpiredTime = DateTime.MaxValue;
                else
                    ExpiredTime = now.AddSeconds(expire);
            }

            /// <summary>更新访问时间并返回数值</summary>
            /// <returns></returns>
            public object Visit()
            {
                VisitTime = DateTime.Now;
                return Value;
            }

            /// <summary>递增</summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public object Inc(object value)
            {
                var code = Type.GetTypeCode(value.GetType());
                // 原子操作
                object newValue, oldValue;
                do
                {
                    oldValue = _Value ?? 0;
                    switch (code)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                            newValue = Convert.ToInt64(oldValue) + Convert.ToInt64(value);
                            break;
                        case TypeCode.Single:
                        case TypeCode.Double:
                            newValue = Convert.ToDouble(oldValue) + Convert.ToDouble(value);
                            break;
                        default:
                            throw new NotSupportedException($"不支持类型[{value.GetType().FullName}]的递增");
                    }
                } while (Interlocked.CompareExchange(ref _Value, newValue, oldValue) != oldValue);

                Visit();

                return newValue;
            }

            /// <summary>递减</summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public object Dec(object value)
            {
                var code = Type.GetTypeCode(value.GetType());
                // 原子操作
                object newValue;
                object oldValue;
                do
                {
                    oldValue = _Value ?? 0;
                    switch (code)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                            newValue = Convert.ToInt64(oldValue) - Convert.ToInt64(value);
                            break;
                        case TypeCode.Single:
                        case TypeCode.Double:
                            newValue = Convert.ToDouble(oldValue) - Convert.ToDouble(value);
                            break;
                        default:
                            throw new NotSupportedException($"不支持类型[{value.GetType().FullName}]的递减");
                    }
                } while (Interlocked.CompareExchange(ref _Value, newValue, oldValue) != oldValue);

                Visit();

                return newValue;
            }
        }
        #endregion

        #region 清理过期缓存

        /// <summary>移除过期的缓存项</summary>
        private void RemoveNotAlive()
        {
            var dic = _cache;
            if (_count == 0 && !dic.Any()) return;

            // 过期时间升序，用于缓存满以后删除
            var slist = new SortedList<DateTime, IList<string>>();
            // 超出个数
            var flag = true;
            if (Capacity <= 0 || _count <= Capacity) flag = false;

            // 60分钟之内过期的数据，进入LRU淘汰
            var now = DateTime.Now;
            var exp = now.AddSeconds(3600);
            var k = 0;

            // 这里先计算，性能很重要
            var list = new List<string>();
            foreach (var item in dic)
            {
                var ci = item.Value;
                if (ci.ExpiredTime <= now)
                    list.Add(item.Key);
                else
                {
                    k++;
                    if (flag && ci.ExpiredTime < exp)
                    {
                        if (!slist.TryGetValue(ci.VisitTime, out var ss))
                            slist.Add(ci.VisitTime, ss = new List<string>());

                        ss.Add(item.Key);
                    }
                }
            }

            // 如果满了，删除前面
            if (flag && slist.Count > 0 && _count - list.Count > Capacity)
            {
                var over = _count - list.Count - Capacity;
                for (var i = 0; i < slist.Count && over > 0; i++)
                {
                    var ss = slist.Values[i];
                    if (ss != null && ss.Count > 0)
                    {
                        foreach (var item in ss)
                        {
                            if (over <= 0) break;

                            list.Add(item);
                            over--;
                            k--;
                        }
                    }
                }

                Debug.WriteLine("[{0}]满，{1:n0}>{2:n0}，删除[{3:n0}]个", Name, _count, Capacity, list.Count);
            }

            foreach (var item in list)
            {
                OnExpire(item);
                _cache.TryRemove(item, out _);
            }

            // 修正
            _count = k;
        }

        /// <summary>缓存过期</summary>
        /// <param name="key"></param>
        protected virtual void OnExpire(string key) => KeyExpired?.Invoke(this, new EventArgs<string>(key));
        #endregion
    }

    /// <summary>生产者消费者</summary>
    /// <typeparam name="T"></typeparam>
    public class MemoryQueue<T> : IDisposable, IProducerConsumer<T>
    {
        private readonly IProducerConsumerCollection<T> _collection;
        private readonly SemaphoreSlim _occupiedNodes;

        /// <summary>实例化内存队列</summary>
        public MemoryQueue()
        {
            _collection = new ConcurrentQueue<T>();
            _occupiedNodes = new SemaphoreSlim(0);
        }

        /// <summary>实例化内存队列</summary>
        /// <param name="collection"></param>
        public MemoryQueue(IProducerConsumerCollection<T> collection)
        {
            _collection = collection;
            _occupiedNodes = new SemaphoreSlim(collection.Count);
        }

        /// <summary>元素个数</summary>
        public int Count => _collection.Count;

        /// <summary>集合是否为空</summary>
        public bool IsEmpty
        {
            get
            {
                if (_collection is ConcurrentQueue<T> queue) return queue.IsEmpty;
                if (_collection is ConcurrentStack<T> stack) return stack.IsEmpty;

                throw new NotSupportedException();
            }
        }

        /// <summary>销毁</summary>
        void IDisposable.Dispose()
        {
            _occupiedNodes.Dispose();
        }

        /// <summary>生产添加</summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public int Add(params T[] values)
        {
            var count = 0;
            foreach (var item in values)
            {
                if (_collection.TryAdd(item))
                {
                    count++;
                    _occupiedNodes.Release();
                }
            }

            return count;
        }

        /// <summary>消费获取</summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<T> Take(int count = 1)
        {
            if (count <= 0) yield break;

            for (var i = 0; i < count; i++)
            {
                if (!_occupiedNodes.Wait(0)) break;
                if (!_collection.TryTake(out var item)) break;

                yield return item;
            }
        }

        /// <summary>消费一个</summary>
        /// <param name="timeout">超时。默认0秒，永久等待</param>
        /// <returns></returns>
        public T TakeOne(int timeout = 0)
        {
            if (!_occupiedNodes.Wait(0))
            {
                if (timeout <= 0 || !_occupiedNodes.Wait(timeout * 1000)) return default;
            }

            return _collection.TryTake(out var item) ? item : default;
        }

        /// <summary>消费获取，异步阻塞</summary>
        /// <param name="timeout">超时。默认0秒，永久等待</param>
        /// <returns></returns>
        public async Task<T> TakeOneAsync(int timeout = 0)
        {
            if (!_occupiedNodes.Wait(0))
            {
                if (timeout <= 0) return default;

                if (!await _occupiedNodes.WaitAsync(timeout * 1000)) return default;
            }

            return _collection.TryTake(out var item) ? item : default;
        }

        /// <summary>消费获取，异步阻塞</summary>
        /// <param name="timeout">超时。默认0秒，永久等待</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        public async Task<T> TakeOneAsync(int timeout, CancellationToken cancellationToken)
        {
            if (!_occupiedNodes.Wait(0))
            {
                if (timeout <= 0) return default;

                if (!await _occupiedNodes.WaitAsync(timeout * 1000, cancellationToken)) return default;
            }

            return _collection.TryTake(out var item) ? item : default;
        }

        /// <summary>确认消费</summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public int Acknowledge(params string[] keys) => 0;
    }
}
