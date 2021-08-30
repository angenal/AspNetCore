using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlSugar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WebFramework.Data
{
    // https://www.donet5.com/home/doc
    /// <summary>
    /// 数据表 基类 inheritors of SqlSugar Client
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbTable<T> : SimpleClient<T> where T : class, new()
    {
        /// <summary>
        /// 数据表 基类
        /// </summary>
        /// <param name="context"></param>
        /// <param name="connectionString"></param>
        /// <param name="separatorChars"></param>
        public DbTable(ISqlSugarClient context = null, ConnectionStrings connectionString = null, string separatorChars = ":") : base(context)
        {
            if (context != null || connectionString == null) return;
            Context = connectionString.DefaultConnection.NewSqlSugarClient(separatorChars);
        }
    }
    /// <summary>
    /// 当前线程的数据库访问类 for SqlSugar Client
    /// </summary>
    public class ThreadLocalSqlSugar
    {
        /// <summary>
        /// 获取当前线程的 SqlSugar Client
        /// </summary>
        public static SqlSugarClient Get(string connectionString, string separatorChars = ":")
        {
            var key = threadLocal.Value;
            if (!cache.ContainsKey(key)) cache.TryAdd(key, connectionString.NewSqlSugarClient(separatorChars));
            return cache[key];
        }
        /// <summary></summary>
        static ThreadLocalSqlSugar() => threadLocal = new ThreadLocal<string>(() => Guid.NewGuid().ToString("N"));
        /// <summary></summary>
        private static readonly ThreadLocal<string> threadLocal;
        /// <summary></summary>
        private static readonly ConcurrentDictionary<string, SqlSugarClient> cache = new ConcurrentDictionary<string, SqlSugarClient>();
    }
    /// <summary>
    /// 数据库访问类 访问上下文 for SqlSugar Client
    /// </summary>
    public class DbContextOfSqlSugar
    {
        /// <summary>
        /// 获取数据库访问类 访问上下文
        /// </summary>
        public SqlSugarClient Client;
        /// <summary>
        /// 创建数据库访问类 访问上下文
        /// </summary>
        /// <param name="options"></param>
        public DbContextOfSqlSugar(IOptions<ConnectionConfig> options) => Client = new SqlSugarClient(options.Value);
    }
    /// <summary>
    /// 数据库访问类 注册到服务容器 for Services Container
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
    /// 数据库内存缓存 for SqlSugar Client
    /// </summary>
    public class SqlSugarMemoryCache : ICacheService
    {
        /// <summary></summary>
        static readonly MemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        /// <summary></summary>
        static readonly ConcurrentDictionary<string, byte> keys = new ConcurrentDictionary<string, byte>();

        /// <summary></summary>
        public void Add<V>(string key, V value)
        {
            cache.Set(key, value);
            keys.TryAdd(key, 0);
        }

        /// <summary></summary>
        public void Add<V>(string key, V value, int cacheDurationInSeconds)
        {
            cache.Set(key, value, TimeSpan.FromSeconds(cacheDurationInSeconds));
            keys.TryAdd(key, 0);
        }

        /// <summary></summary>
        public bool ContainsKey<V>(string key) => cache.TryGetValue(key, out _);

        /// <summary></summary>
        public V Get<V>(string key) => cache.Get<V>(key);

        /// <summary></summary>
        public IEnumerable<string> GetAllKey<V>() => keys.Keys;

        /// <summary></summary>
        public V GetOrCreate<V>(string cacheKey, Func<V> create, int cacheDurationInSeconds = int.MaxValue)
        {
            if (cache.TryGetValue(cacheKey, out object v) && v != null) return (V)v;
            var value = create();
            Add(cacheKey, value, cacheDurationInSeconds);
            return value;
        }

        /// <summary></summary>
        public void Remove<V>(string key)
        {
            cache.Remove(key);
            keys.TryRemove(key, out _);
        }
    }
    /// <summary>
    /// 数据库访问类 扩展方法
    /// </summary>
    public static class SqlSugarExtensions
    {
        /// <summary>
        /// new SqlSugarClient
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="separatorChars"></param>
        /// <returns></returns>
        public static SqlSugarClient NewSqlSugarClient(this string connectionString, string separatorChars = ":")
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            DbType dbType = DbType.SqlServer;
            if (connectionString.Length > 11 && !string.IsNullOrEmpty(separatorChars))
            {
                var s = connectionString.Substring(0, 11);
                var i = s.IndexOf(separatorChars);
                if (i > 1)
                {
                    var privoder = s.Substring(0, i);
                    switch (privoder)
                    {
                        case "sqlserver":
                        case "mssql":
                            dbType = DbType.SqlServer;
                            break;
                        case "mysql":
                            dbType = DbType.MySql;
                            break;
                        case "oracle":
                            dbType = DbType.Oracle;
                            break;
                        case "sqlite":
                        case "sqlite3":
                            dbType = DbType.Sqlite;
                            break;
                        case "postgresql":
                            dbType = DbType.PostgreSQL;
                            break;
                        default:
                            throw new ArgumentException("Unsupported database", nameof(connectionString));
                    }
                    connectionString = connectionString.Substring(i + separatorChars.Length).Trim();
                }
            }

            var db = new SqlSugarClient(new ConnectionConfig()
            {
                DbType = dbType,
                ConnectionString = connectionString,
                // Init Entities Attribute: [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true,
                MoreSettings = new ConnMoreSettings() { IsAutoRemoveDataCache = true },
                ConfigureExternalServices = new ConfigureExternalServices() { DataInfoCacheService = new SqlSugarMemoryCache() }
            });
#if DEBUG
            //db.Ado.IsEnableLogEvent = true;
            return db.Debug();
#else
            db.Ado.IsEnableLogEvent = false;
            return db;
#endif
        }
        /// <summary>
        /// 调式代码,打印SQL
        /// </summary>
        /// <param name="db"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static SqlSugarClient Debug(this SqlSugarClient db, Action<string> action = null)
        {
            if (db == null) return db;
            if (action == null) action = sql => System.Diagnostics.Debug.WriteLine(sql);
            if (db.Aop != null) db.Aop.OnLogExecuting = (sql, pars) => action.Invoke("  " + sql + Environment.NewLine + WriteLineSugarParameter(pars));
            return db;
        }
        static string WriteLineSugarParameter(SugarParameter[] pars)
        {
            if (pars == null) return null;
            var s = new StringBuilder();
            foreach (SugarParameter p in pars) s.Append($"    {p.ParameterName} = {p.Value} {Environment.NewLine}");
            return s.ToString();
        }
        //public static IApplicationBuilder UseSqlSugar(this IApplicationBuilder builder) => builder.UseMiddleware<SqlSugarMiddleware>();
    }
}
