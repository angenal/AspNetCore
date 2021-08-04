using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace WebFramework.Data
{
    /// <summary>
    /// 数据表 基类 https://www.donet5.com/home/doc
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbTable<T> : SimpleClient<T> where T : class, new()
    {
        /// <summary>
        /// 数据表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="connection"></param>
        public DbTable(ISqlSugarClient context = null, ConnectionStrings connection = null) : base(context)
        {
            if (context != null || connection == null) return;
            Context = new SqlSugarClient(new ConnectionConfig()
            {
                DbType = DbType.SqlServer,
                ConnectionString = connection.DefaultConnection,
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true,
                MoreSettings = new ConnMoreSettings() { IsAutoRemoveDataCache = true },
                ConfigureExternalServices = new ConfigureExternalServices() { DataInfoCacheService = new RedisCache() }
            });
        }
    }
    /// <summary>
    /// 数据库 访问上下文
    /// </summary>
    public class DbContextOfSqlSugar
    {
        /// <summary>
        /// 访问上下文
        /// </summary>
        public SqlSugarClient Client;
        /// <summary>
        ///
        /// </summary>
        /// <param name="options"></param>
        public DbContextOfSqlSugar(IOptions<ConnectionConfig> options) => Client = new SqlSugarClient(options.Value);
    }
    /// <summary>
    /// 把数据库访问类 注册到服务容器中
    /// </summary>
    public static class SqlSugarServiceCollectionExtensions
    {
        /// <summary>
        /// 注册到服务容器中
        /// </summary>
        public static IServiceCollection AddDbContextOfSqlSugar<TContext>(this IServiceCollection services, Action<ConnectionConfig> optionsAction = null) where TContext : DbContextOfSqlSugar
        {
            if (optionsAction != null) services.Configure(optionsAction);
            services.AddScoped<TContext>();
            return services;
        }
    }
    /// <summary>
    /// 数据库 缓存Redis
    /// </summary>
    public class RedisCache : ICacheService
    {
        public static MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        public void Add<V>(string key, V value)
        {
            Cache.Set(key, value);
        }

        public void Add<V>(string key, V value, int cacheDurationInSeconds)
        {
            Cache.Set(key, value, TimeSpan.FromSeconds(cacheDurationInSeconds));
        }

        public bool ContainsKey<V>(string key)
        {
            return Cache.TryGetValue(key, out _);
        }

        public V Get<V>(string key)
        {
            return Cache.Get<V>(key);
        }

        public IEnumerable<string> GetAllKey<V>()
        {
            return new string[0];
        }

        public V GetOrCreate<V>(string cacheKey, Func<V> create, int cacheDurationInSeconds = int.MaxValue)
        {
            if (Cache.TryGetValue(cacheKey, out object v))
            {
                return (V)v;
            }
            else
            {
                var result = create();
                Add(cacheKey, result, cacheDurationInSeconds);
                return result;
            }
        }

        public void Remove<V>(string key)
        {
            Cache.Remove(key.Remove(0, 6));
        }
    }
}
