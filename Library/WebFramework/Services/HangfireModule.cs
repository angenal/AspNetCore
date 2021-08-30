using Hangfire;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Hangfire.LiteDB;
using Hangfire.MemoryStorage;
using Hangfire.Pro.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Net.Http.Headers;
using WebCore;

namespace WebFramework.Services
{
    /* appsettings.json
      "Hangfire": {
        "LiteDB": "Filename=App_Data/Hangfire.db;Password=HGJ766GR767FKJU0",
        "Redis": "127.0.0.1:6379,defaultDatabase=0",
        "Prefix": "hangfire:demo",
        "DashboardTitle": "Hangfire Dashboard",
        "Authorization": {
          "User": "demo",
          "Pass": "demo"
        }
      }
    */

    /// <summary>
    /// Hangfire: Background jobs and workers.
    /// </summary>
    public static class HangfireModule
    {
        /// <summary>
        /// Register services
        /// </summary>
        public static IServiceCollection AddHangfire(this IServiceCollection services, IConfiguration config)
        {
            // Differentiate between different applications
            var prefixSection = config.GetSection("Hangfire:Prefix");
            var prefix = prefixSection.Exists() ? prefixSection.Value : "hangfire";
            var serverName = $"{prefix.Replace(":", "-").Replace("/", "-")}-{Environment.CurrentDirectory.Crc32x8()}";

            // SqlServer Storage
            if (config.GetSection("Hangfire:DB").Exists())
                services.AddHangfire(x => x.UseSqlServerStorage(config.GetSection("Hangfire:DB").Value, new Hangfire.SqlServer.SqlServerStorageOptions
                {
                    SchemaName = "hangfire",                             // 数据库表dbo
                    QueuePollInterval = TimeSpan.FromSeconds(15),        // 作业队列轮询间隔 默认15秒
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),  // 作业到期检查间隔（管理过期记录）默认1小时
                    CountersAggregateInterval = TimeSpan.FromMinutes(5), // 聚合计数器的间隔 默认5分钟
                    TransactionTimeout = TimeSpan.FromMinutes(5),        // 交易超时 默认5分钟
                    PrepareSchemaIfNecessary = true,                     // 如果设置为true 则创建数据库表 默认true
                    DashboardJobListLimit = 50000,                       // 仪表板作业列表限制 默认50000
                    //InvisibilityTimeout = TimeSpan.FromMinutes(5),     // 超时后由另一个工作进程接手该后台作业任务（重新加入）默认5分钟
                }));
            // LiteDb Storage
            else if (config.GetSection("Hangfire:LiteDB").Exists())
                services.AddHangfire(x => x.UseLiteDbStorage(config.GetSection("Hangfire:LiteDB").Value, new LiteDbStorageOptions
                {
                    //Prefix = "hangfire",                               // 数据表前缀 默认hangfire
                    DashboardTitle = "存储: LiteDB",                     // 数据库描述（控制台底部显示）
                    QueuePollInterval = TimeSpan.FromSeconds(15),        // 作业队列轮询间隔 默认15秒
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),  // 作业到期检查间隔（管理过期记录）默认1小时
                    CountersAggregateInterval = TimeSpan.FromMinutes(5), // 聚合计数器的间隔 默认5分钟
                    DistributedLockLifetime = TimeSpan.FromSeconds(30),  // 分布式锁的生存期 默认30秒
                    InvisibilityTimeout = TimeSpan.FromMinutes(30),      // 超时后由另一个工作进程接手该后台作业任务（重新加入）默认30分钟
                }));
            // Redis Storage  https://stackexchange.github.io/StackExchange.Redis/Configuration.html
            else if (config.GetSection("Hangfire:Redis").Exists())
                services.AddHangfire(x => x.UseRedisStorage(config.GetSection("Hangfire:Redis").Value, new RedisStorageOptions
                {
                    Prefix = prefix,
                    MaxDeletedListLength = 1000,
                    MaxSucceededListLength = 1000,
                    InvisibilityTimeout = TimeSpan.FromMinutes(5),       // 超时后由另一个工作进程接手该后台作业任务（重新加入）默认5分钟
                }));
            // Hangfire.AspNetCore + HangFire.Redis.StackExchange  https://github.com/marcoCasamento/Hangfire.Redis.StackExchange
            //services.AddHangfire(x => x.UseRedisStorage(StackExchange.Redis.ConnectionMultiplexer.Connect(config.GetSection("Hangfire:Redis").Value), new RedisStorageOptions
            //{
            //    Prefix = config.GetSection("Hangfire:Prefix").Value,
            //    //InvisibilityTimeout = TimeSpan.FromMinutes(5),       // 超时后由另一个工作进程接手该后台作业任务（重新加入）默认5分钟
            //}));
            // Memory Storage
            else
            {
                services.AddHangfire(x => x.UseMemoryStorage(new MemoryStorageOptions
                {
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),   // 作业到期检查间隔（管理过期记录）默认1小时
                    CountersAggregateInterval = TimeSpan.FromMinutes(5),  // 聚合计数器的间隔 默认5分钟
                }));
                services.AddHangfireServer(options =>
                {
                    options.Queues = new[] { "default" };
                    options.ServerName = $"{serverName}-{Environment.ProcessId}";
                    options.TimeZoneResolver = new HangfireResolvers();   // 本地时区
                    options.WorkerCount = Environment.ProcessorCount;     // 并发任务数
                });
                return services;
            }

            services.AddHangfireServer(options =>
            {
                options.Queues = new[] { "default" };
                options.ServerName = serverName;
                options.TimeZoneResolver = new HangfireResolvers();   // 本地时区
                options.WorkerCount = Environment.ProcessorCount * 5; // 并发任务数
            });


            return services;
        }

        /// <summary>
        /// Configure services
        /// </summary>
        public static IApplicationBuilder UseHangfire(this IApplicationBuilder app, IConfiguration config)
        {
            //管理账号
            string userSection = "Hangfire:Authorization:User", passSection = "Hangfire:Authorization:Pass";
            var auth = new HangfireBasicAuthenticationFilter()
            {
                DashboardTitle = config.GetSection("Hangfire:DashboardTitle")?.Value ?? "Hangfire Dashboard",
                User = config.GetSection(userSection)?.Value,
                Pass = config.GetSection(passSection)?.Value,
            };
            string user = Environment.GetEnvironmentVariable(userSection.Replace(":", "_")), pass = Environment.GetEnvironmentVariable(passSection.Replace(":", "_"));
            if (!string.IsNullOrWhiteSpace(user)) auth.User = user; if (!string.IsNullOrWhiteSpace(pass)) auth.Pass = pass;
            var resolver = new HangfireResolvers();
            //管理面板
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { auth },
                DashboardTitle = auth.DashboardTitle,
                IsReadOnlyFunc = resolver.IsReadOnlyFunc,
                IgnoreAntiforgeryToken = true,
                TimeZoneResolver = resolver
            });
            //启动服务
            app.UseHangfireServer();

            return app;
        }
    }

    /// <summary></summary>
    public class HangfireResolvers : IDashboardAuthorizationFilter, ITimeZoneResolver
    {
        /// <summary></summary>
        public bool Authorize([NotNull] DashboardContext context)
        {
            string ip = context.Request.LocalIpAddress;
            return ip.Equals("127.0.0.1") || ip.Equals("::1");
        }

        /// <summary></summary>
        public bool IsReadOnlyFunc([NotNull] DashboardContext context) => !Authorize(context);

        /// <summary></summary>
        public TimeZoneInfo GetTimeZoneById([NotNull] string timeZoneId) => TimeZoneInfo.Local;
    }

    /// <summary></summary>
    public class HangfireBasicAuthenticationTokens
    {
        private readonly string[] _tokens;

        /// <summary></summary>
        public string User => _tokens[0];
        /// <summary></summary>
        public string Pass => _tokens[1];

        /// <summary></summary>
        public HangfireBasicAuthenticationTokens(string[] tokens) => _tokens = tokens;

        /// <summary></summary>
        public bool IsInvalid() => _tokens.Length == 2 && string.IsNullOrWhiteSpace(User) && string.IsNullOrWhiteSpace(Pass);

        /// <summary></summary>
        public bool IsMatch(string user, string pass) => User.Equals(user) && Pass.Equals(pass);
    }
    /// <summary></summary>
    public class HangfireBasicAuthenticationFilter : IDashboardAuthorizationFilter
    {
        /// <summary></summary>
        const string Scheme = "Basic";
        /// <summary></summary>
        public string DashboardTitle { get; set; }
        /// <summary></summary>
        public string User { get; set; }
        /// <summary></summary>
        public string Pass { get; set; }

        /// <summary></summary>
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var header = httpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(header))
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            var authValues = AuthenticationHeaderValue.Parse(header);

            if (Not_Basic_Authentication(authValues))
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            var tokens = ExtractTokens(authValues);

            if (tokens.IsInvalid())
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            if (tokens.IsMatch(User, Pass))
            {
                return true;
            }

            SetChallengeResponse(httpContext);
            return false;
        }

        /// <summary></summary>
        void SetChallengeResponse(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = 401;
            httpContext.Response.Headers.Append("WWW-Authenticate", $"Basic realm=\"{DashboardTitle}\"");
            httpContext.Response.WriteAsync("Authentication is required.");
        }

        /// <summary></summary>
        static HangfireBasicAuthenticationTokens ExtractTokens(AuthenticationHeaderValue authValues)
        {
            var parameter = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
            return new HangfireBasicAuthenticationTokens(parameter.Split(':'));
        }

        /// <summary></summary>
        static bool Not_Basic_Authentication(AuthenticationHeaderValue authValues)
        {
            return !Scheme.Equals(authValues.Scheme, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
