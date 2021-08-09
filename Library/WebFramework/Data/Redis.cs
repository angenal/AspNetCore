using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Authentication;

namespace WebFramework.Data
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    /// <summary>
    /// Redis configuration
    /// </summary>
    public class RedisConfiguration
    {
        private readonly string _hostname;

        public const int DefaultSyncTimeout = 5000;
        public const int DefaultConnectTimeout = 5000;
        public const int DefaultConnectRetry = 3;
        public const int DefaultConfigCheckSeconds = 60;
        public const int DefaultWriteBuffer = 4096;

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
        public int? ConnectRetry { get; set; } = DefaultConnectRetry;

        /// <summary>
        /// Timeout (ms) for connect operations
        /// </summary>
        public int? ConnectTimeout { get; set; } = DefaultConnectTimeout;

        /// <summary>
        /// Broadcast channel name for communicating configuration changes
        /// </summary>
        public string ConfigChannel { get; set; }

        /// <summary>
        /// Time (seconds) to check configuration. This serves as a keep-alive for interactive sockets, if it is supported.
        /// </summary>
        public int? ConfigCheckSeconds { get; set; } = DefaultConfigCheckSeconds;

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
        public int? ResponseTimeout { get; set; } = DefaultSyncTimeout;

        /// <summary>Not currently implemented (intended for use with sentinel)
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
        public int? SyncTimeout { get; set; } = DefaultSyncTimeout;

        /// <summary>
        /// Time (ms) to allow for asynchronous operations
        /// </summary>
        public int? AsyncTimeout { get; set; } = DefaultSyncTimeout;

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
        public int? WriteBuffer { get; set; } = DefaultWriteBuffer;

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

    /// <summary>
    /// Redis db manager
    /// </summary>
    public static class RedisManager
    {
        /// <summary>
        /// Redis server connection string
        /// </summary>
        private const string DefaultConfiguration = "localhost:6379";

        /// <summary>
        /// Redis database
        /// </summary>
        private const int DataBase = -1;

        /// <summary>
        /// The redis configuration string
        /// </summary>
        private static string _configuration = DefaultConfiguration;

        /// <summary>
        /// The connection multiplexer.
        /// </summary>
        private static Lazy<ConnectionMultiplexer> _connectionMultiplexer;

        /// <summary>
        /// Get the redis connection multiplexer once
        /// </summary>
        private static readonly Lazy<IConnectionMultiplexer> redis = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_configuration));

        /// <summary>
        /// Get redis db instance
        /// </summary>
        public static IDatabase Db => redis.Value.GetDatabase(DataBase);

        /// <summary>
        /// Get redis db instance
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IDatabase GetDatabase(int db = -1) => redis.Value.GetDatabase(db);

        /// <summary>
        /// Get redis client connection
        /// </summary>
        /// <returns></returns>
        public static IConnectionMultiplexer GetConnection() => _connectionMultiplexer?.Value;

        /// <summary>
        /// Init redis cache configuration
        /// </summary>
        /// <param name="configuration"></param>
        public static void Init(string configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Init redis cache configuration
        /// </summary>
        /// <param name="configuration"></param>
        public static void Init(RedisConfiguration configuration)
        {
            _configuration = configuration.ToString();
        }

        public static void Init(EasyCaching.Redis.RedisDBOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Configuration))
            {
                var configurationOptions = new ConfigurationOptions
                {
                    ConnectTimeout = options.ConnectionTimeout,
                    User = options.Username,
                    Password = options.Password,
                    Ssl = options.IsSsl,
                    SslHost = options.SslHost,
                    AllowAdmin = options.AllowAdmin,
                    DefaultDatabase = options.Database,
                    AbortOnConnectFail = options.AbortOnConnectFail,
                };

                foreach (var endpoint in options.Endpoints)
                {
                    configurationOptions.EndPoints.Add(endpoint.Host, endpoint.Port);
                }

                _configuration = configurationOptions.ToString();
            }
            else
            {
                _configuration = options.Configuration;
            }

            _connectionMultiplexer = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_configuration));
        }

        /// <summary>
        /// Gets the server list.
        /// </summary>
        /// <returns>The server list.</returns>
        public static IEnumerable<IServer> GetServerList()
        {
            var endpoints = GetMastersServersEndpoints();

            foreach (var endpoint in endpoints)
            {
                yield return _connectionMultiplexer.Value.GetServer(endpoint);
            }
        }

        /// <summary>
        /// Gets the masters servers endpoints.
        /// </summary>
        public static List<EndPoint> GetMastersServersEndpoints()
        {
            var masters = new List<EndPoint>();
            if (_connectionMultiplexer == null) return masters;
            foreach (var ep in _connectionMultiplexer.Value.GetEndPoints())
            {
                var server = _connectionMultiplexer.Value.GetServer(ep);
                if (server.IsConnected)
                {
                    //Cluster
                    if (server.ServerType == ServerType.Cluster)
                    {
                        masters.AddRange(server.ClusterConfiguration.Nodes.Where(n => !n.IsReplica).Select(n => n.EndPoint));
                        break;
                    }
                    // Single , Master-Slave
                    if (server.ServerType == ServerType.Standalone && !server.IsReplica)
                    {
                        masters.Add(ep);
                        break;
                    }
                }
            }
            return masters;
        }
    }
}
