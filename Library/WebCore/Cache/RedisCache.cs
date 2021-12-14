using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebInterface;

namespace WebCore.Cache
{
    /// <summary>
    /// Redis缓存
    /// </summary>
    /// <remarks>
    /// 依赖库 https://www.nuget.org/packages/CSRedisCore
    /// </remarks>
    public class RedisCache : Cache
    {
        #region 属性
        public CSRedis.CSRedisClient CSRedis { get; set; }
        #endregion

        #region 静态默认实现
        /// <summary>默认缓存</summary>
        public static ICache Instance { get; set; } = new RedisCache("127.0.0.1:6379");
        #endregion

        #region 构造
        public RedisCache(string connectionstring)
        {
            CSRedis = new CSRedis.CSRedisClient(connectionstring);
        }
        public RedisCache(string masterConnectionstring, string[] sentinels, bool readOnly = false)
        {
            CSRedis = new CSRedis.CSRedisClient(masterConnectionstring, sentinels, readOnly);
        }
        /// <summary>
        /// 新建Redis缓存实例
        /// </summary>
        /// <param name="server">服务器IP地址</param>
        /// <param name="password">密码</param>
        /// <param name="defaultDatabase">连接默认数据库ID</param>
        /// <param name="poolsize">连接池大小</param>
        /// <param name="preheat">预热连接,接收值,例如预热=5预热5个连接</param>
        /// <param name="tryit">执行错误后重试次数</param>
        /// <param name="idleTimeout">连接池中元素的空闲时间(毫秒) 适合连接到远程服务器</param>
        /// <param name="connectTimeout">连接超时(毫秒)</param>
        /// <param name="syncTimeout">命令执行超时(毫秒)</param>
        /// <param name="ssl">启用加密传输</param>
        /// <param name="testcluster">是否尝试集群模式,阿里云,腾讯云集群需要设置此选项为 false</param>
        /// <param name="name">连接名称,使用客户端列表命令查看</param>
        /// <param name="prefix">key前辍,所有方法都会附带此前辍, redis.Set(prefix + "key", 111);</param>
        public RedisCache(string server, string password = "", int defaultDatabase = 0,
            int poolsize = 50, int preheat = 5, int tryit = 0,
            int idleTimeout = 20000, int connectTimeout = 5000, int syncTimeout = 10000,
            bool ssl = false, bool testcluster = true, string name = "", string prefix = "")
        {
            var s = new StringBuilder(server);
            if (!string.IsNullOrEmpty(password)) s.Append($",password={password}");
            s.Append($",defaultDatabase={defaultDatabase}");
            s.Append($",poolsize={poolsize}");
            if (preheat > 0) s.Append($",preheat={preheat}");
            if (tryit > 0) s.Append($",tryit={tryit}");
            if (idleTimeout > 0) s.Append($",idleTimeout={idleTimeout}");
            if (connectTimeout > 0) s.Append($",connectTimeout={connectTimeout}");
            if (syncTimeout > 0) s.Append($",syncTimeout={syncTimeout}");
            if (ssl) s.Append(",ssl=true");
            s.Append($",testcluster={testcluster}");
            if (!string.IsNullOrEmpty(name)) s.Append($",name={name}");
            if (!string.IsNullOrEmpty(prefix)) s.Append($",prefix={prefix}");
            CSRedis = new CSRedis.CSRedisClient(s.ToString());
        }
        /// <summary>已重载。</summary>
        /// <returns></returns>
        public override string ToString() => CSRedis.ToString();
        #endregion

        #region 基础操作
        /// <summary>缓存个数</summary>
        public override int Count => CSRedis.Nodes.Count;

        /// <summary>获取所有键，相当不安全，禁止使用。</summary>
        public override ICollection<string> Keys => CSRedis.Keys("*");

        /// <summary>单个实体项</summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expire">过期时间，秒。小于0时采用默认缓存时间<seealso cref="Cache.Expire"/></param>
        public override bool Set<T>(string key, T value, int expire = -1) => CSRedis.Set(key, value, expire);

        /// <summary>获取单体</summary>
        /// <param name="key">键</param>
        public override T Get<T>(string key) => CSRedis.Get<T>(key);

        /// <summary>批量移除缓存项</summary>
        /// <param name="keys">键集合</param>
        public override int Remove(params string[] keys) => (int)CSRedis.Del(keys);

        /// <summary>清空所有缓存项</summary>
        public override void Clear() => CSRedis.Eval("FLUSHDB", "");

        /// <summary>是否存在</summary>
        /// <param name="key">键</param>
        public override bool ContainsKey(string key) => CSRedis.Exists(key);

        /// <summary>设置缓存项有效期</summary>
        /// <param name="key">键</param>
        /// <param name="expire">过期时间</param>
        public override bool SetExpire(string key, TimeSpan expire) => CSRedis.Expire(key, (int)expire.TotalSeconds);

        /// <summary>获取缓存项有效期</summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public override TimeSpan GetExpire(string key) => TimeSpan.FromSeconds(CSRedis.Ttl(key));
        #endregion

        #region 集合操作
        /// <summary>批量获取缓存项</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public override IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            string[] ks = keys.ToArray();
            T[] vs = CSRedis.MGet<T>(ks);
            var result = new Dictionary<string, T>();
            for (int i = 0; i < ks.Length; i++) result[ks[i]] = vs[i];
            return result;
        }

        /// <summary>批量设置缓存项</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="expire">过期时间，秒。小于0时采用默认缓存时间<seealso cref="Cache.Expire"/></param>
        public override void SetAll<T>(IDictionary<string, T> values, int expire = -1)
        {
            if (values == null || values.Count == 0) return;

            if (expire < 0) expire = Expire;

            // 优化少量缓存
            if (values.Count <= 3)
            {
                foreach (var item in values)
                {
                    Set(item.Key, item.Value, expire);
                }
                return;
            }

            // 使用管道批量设置
            var pipe = CSRedis.StartPipe();
            try
            {
                foreach (var item in values)
                {
                    pipe.Set(item.Key, item.Value, expire);
                }
            }
            finally
            {
                pipe.EndPipe();
            }
        }

        /// <summary>获取哈希</summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public override IDictionary<string, T> GetDictionary<T>(string key) => throw new NotSupportedException("Redis未支持该功能");

        /// <summary>获取队列</summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public override IProducerConsumer<T> GetQueue<T>(string key) => throw new NotSupportedException("Redis未支持该功能");

        /// <summary>获取栈</summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public override IProducerConsumer<T> GetStack<T>(string key) => throw new NotSupportedException("Redis未支持该功能");

        /// <summary>获取Set</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public override ICollection<T> GetSet<T>(string key) => throw new NotSupportedException("Redis未支持该功能");
        #endregion

        #region 高级操作
        /// <summary>添加，已存在时不更新</summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expire">过期时间，秒。小于0时采用默认缓存时间<seealso cref="Cache.Expire"/></param>
        /// <returns></returns>
        public override bool Add<T>(string key, T value, int expire = -1)
        {
            if (expire < 0) expire = Expire;

            // 没有有效期，直接使用SETNX
            if (expire <= 0) return CSRedis.SetNx(key, value);

            // 带有有效期
            return CSRedis.SetNx(key, value) && CSRedis.Expire(key, expire);
        }

        /// <summary>设置新值并获取旧值，原子操作</summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public override T Replace<T>(string key, T value) => CSRedis.GetSet<T>(key, value);

        /// <summary>尝试获取指定键，返回是否包含值。有可能缓存项刚好是默认值，或者只是反序列化失败</summary>
        /// <remarks>
        /// 在 Redis 中，可能有key（此时TryGet返回true），但是因为反序列化失败，从而得不到value。
        /// </remarks>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值。即使有值也不一定能够返回，可能缓存项刚好是默认值，或者只是反序列化失败</param>
        /// <returns>返回是否包含值，即使反序列化失败</returns>
        public override bool TryGetValue<T>(string key, out T value)
        {
            T v0 = default, v1 = CSRedis.Get<T>(key);
            value = v1;
            return !Equals(v0, v1);
        }

        /// <summary>累加，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public override long Increment(string key, long value) => CSRedis.IncrBy(key, value);

        /// <summary>累加，原子操作，乘以100后按整数操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public override double Increment(string key, double value) => (double)CSRedis.IncrByFloat(key, (decimal)value);

        /// <summary>递减，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public override long Decrement(string key, long value) => Increment(key, -value);

        /// <summary>递减，原子操作</summary>
        /// <param name="key">键</param>
        /// <param name="value">变化量</param>
        /// <returns></returns>
        public override double Decrement(string key, double value) => Increment(key, -value);
        #endregion
    }
}
