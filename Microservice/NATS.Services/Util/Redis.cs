using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Authentication;

namespace NATS.Services.Util
{
    public class Redis
    {
        /// <summary>
        /// Redis database
        /// </summary>
        private const int DataBase = -1;

        /// <summary>
        /// The Redis configuration string
        /// </summary>
        private static string _configuration = null;

        /// <summary>
        /// Get the Redis connection multiplexer once
        /// </summary>
        private static readonly Lazy<IConnectionMultiplexer> redis = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_configuration));

        private static bool inited = false;

        public static int DefaultExpirationSeconds = 0;

        public static void Init(Config.RedisConfig config, int defaultExpirationSeconds = 3600)
        {
            if (_configuration != null || inited) return;

            _configuration = new RedisConfiguration(config.Addr) { Password = config.Password, DefaultDatabase = config.Db }.ToString();

            DefaultExpirationSeconds = defaultExpirationSeconds;
        }

        public static void Configure(Action<string> action)
        {
            if (_configuration == null || inited) return;

            action.Invoke(_configuration);

            inited = true;
        }

        public class List
        {
            /// <summary>
            /// Redis db instance
            /// </summary>
            readonly IDatabase Db;

            public List(string configuration = null)
            {
                Db = string.IsNullOrEmpty(configuration) ? redis.Value.GetDatabase(DataBase) : ConnectionMultiplexer.Connect(configuration).GetDatabase();
            }

            public string Get(string key)
            {
                var redisKey = new RedisKey(key);
                var redisVal = Db.StringGet(redisKey);
                return redisVal.HasValue ? redisVal.ToString() : null;
            }

            public bool Set(string key, string value, int expire)
            {
                var redisKey = new RedisKey(key);
                return Db.StringSet(redisKey, new RedisValue(value), expire > 0 ? TimeSpan.FromSeconds(expire) : null);
            }

            public long Push(string key, string value, int expire)
            {
                var redisKey = new RedisKey(key);
                var i = Db.ListRightPush(redisKey, new RedisValue(value));
                if (expire > 0) Db.KeyExpire(redisKey, TimeSpan.FromSeconds(expire));
                return i;
            }

            public long Push<T>(string key, T value, int expire)
            {
                var redisKey = new RedisKey(key);
                var i = Db.ListRightPush(redisKey, new RedisValue(JsonConvert.SerializeObject(value, NewtonsoftJson.Converters)));
                if (expire > 0) Db.KeyExpire(redisKey, TimeSpan.FromSeconds(expire));
                return i;
            }

            public long Push<T>(string key, T[] value)
            {
                var l = value.Length;
                if (l == 0) return 0;

                var values = new RedisValue[l];
                for (var i = 0; i < l; i++) values.SetValue(new RedisValue(JsonConvert.SerializeObject(value[i], NewtonsoftJson.Converters)), i);

                var redisKey = new RedisKey(key);
                return Db.ListRightPush(redisKey, values);
            }

            public List<T> Pop<T>(string key, int size = 200)
            {
                var list = new List<T>();
                if (size <= 0) return list;

                try
                {
                    var redisKey = new RedisKey(key);
                    var redisVal = Db.ListLeftPop(redisKey);

                    while (redisVal.HasValue && size > 0)
                    {
                        if (!redisVal.IsNullOrEmpty)
                        {
                            T item = JsonConvert.DeserializeObject<T>(redisVal.ToString());
                            list.Add(item);
                        }

                        redisVal = Db.ListLeftPop(redisKey);
                        size--;
                    }
                }
                catch (Exception) { }
                return list;
            }

            public List<string> Pop(string key, int size = 200)
            {
                var list = new List<string>();
                if (size <= 0) return list;

                try
                {
                    var redisKey = new RedisKey(key);
                    var redisVal = Db.ListLeftPop(redisKey);

                    while (redisVal.HasValue && size > 0)
                    {
                        if (!redisVal.IsNullOrEmpty)
                        {
                            list.Add(redisVal.ToString());
                        }

                        redisVal = Db.ListLeftPop(redisKey);
                        size--;
                    }
                }
                catch (Exception) { }
                return list;
            }

            public bool Expire(string key, int expire)
            {
                var redisKey = new RedisKey(key);
                if (expire > 0) return Db.KeyExpire(redisKey, TimeSpan.FromSeconds(expire));
                return Db.KeyExpire(redisKey, (TimeSpan?)null);
            }

            public TimeSpan? Expire(string key)
            {
                var redisKey = new RedisKey(key);
                return Db.KeyTimeToLive(redisKey);
            }

            public TimeSpan? Idle(string key)
            {
                var redisKey = new RedisKey(key);
                return Db.KeyIdleTime(redisKey);
            }

            public bool Delete(string key)
            {
                return Db.KeyDelete(new RedisKey(key));
            }
        }

    }

    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    /// <summary>
    /// Redis configuration
    /// </summary>
    public class RedisConfiguration
    {
        private string _hostname;

        /// <summary>
        /// If true, Connect will not create a connection while no servers are available
        /// </summary>
        public bool? AbortConnect { get; set; } = true;

        /// <summary>
        /// Enables a range of commands that are considered risky
        /// </summary>
        public bool? AllowAdmin { get; set; }

        /// <summary>
        /// Optional channel prefix for all pub/sub operations
        /// </summary>
        public string ChannelPrefix { get; set; }

        /// <summary>
        /// The number of times to repeat connect attempts during initial Connect
        /// </summary>
        public int? ConnectRetry { get; set; } = 3;

        /// <summary>
        /// Timeout (ms) for connect operations
        /// </summary>
        public int? ConnectTimeout { get; set; } = 5000;

        /// <summary>
        /// Broadcast channel name for communicating configuration changes
        /// </summary>
        public string ConfigChannel { get; set; }

        /// <summary>
        /// Time (seconds) to check configuration. This serves as a keep-alive for interactive sockets, if it is supported.
        /// </summary>
        public int? ConfigCheckSeconds { get; set; } = 60;

        /// <summary>
        /// Default database index, from 0 to databases - 1
        /// </summary>
        public int? DefaultDatabase { get; set; }

        /// <summary>
        /// Time (seconds) at which to send a message to help keep sockets alive (60 sec default)
        /// </summary>
        public int? KeepAlive { get; set; }

        /// <summary>
        /// Identification for the connection within redis
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Password for the redis server
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// User for the redis server (for use with ACLs on redis 6 and above)
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Type of proxy in use (if any); for example “twemproxy”
        /// </summary>
        public Proxy Proxy { get; set; } = Proxy.None;

        /// <summary>
        /// Specifies that DNS resolution should be explicit and eager, rather than implicit
        /// </summary>
        public bool? ResolveDns { get; set; }

        /// <summary>
        /// Time (ms) to decide whether the socket is unhealthy
        /// </summary>
        public int? ResponseTimeout { get; set; } = 5000;

        /// <summary>
        /// Not currently implemented (intended for use with sentinel)
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Specifies that SSL encryption should be used
        /// </summary>
        public bool? Ssl { get; set; }

        /// <summary>
        /// Enforces a particular SSL host identity on the server’s certificate
        /// </summary>
        public string SslHost { get; set; }

        /// <summary>
        /// Ssl/Tls versions supported when using an encrypted connection. Use ‘|’ to provide multiple values.
        /// </summary>
        public SslProtocols? SslProtocols { get; set; }

        /// <summary>
        /// Time (ms) to allow for synchronous operations
        /// </summary>
        public int? SyncTimeout { get; set; } = 5000;

        /// <summary>
        /// Time (ms) to allow for asynchronous operations
        /// </summary>
        public int? AsyncTimeout { get; set; } = 5000;

        /// <summary>
        /// Key to use for selecting a server in an ambiguous master scenario
        /// </summary>
        public string TieBreaker { get; set; }

        /// <summary>
        /// Redis version level (useful when the server does not make this available)
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Size of the output buffer
        /// </summary>
        public int? WriteBuffer { get; set; } = 4096;

        public RedisConfiguration() { }
        public RedisConfiguration(string hostname) { _hostname = hostname; }

        public override string ToString()
        {
            var config = new List<string>();
            var properties = typeof(RedisConfiguration).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                var value = property.GetValue(this);
                if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                    value = value?.ToString().ToLower();
                var name = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);

                if (value != null) config.Add($"{name}={value}");
            }

            return string.IsNullOrEmpty(_hostname) ? string.Join(",", config) : $"{_hostname},{string.Join(",", config)}";
        }
    }
}
