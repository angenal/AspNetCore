using EasyCaching.Core;
using EasyCaching.ResponseCaching;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebFramework.Services
{
    /// <summary>
    /// Cache Module
    /// </summary>
    public static class CacheModule
    {
        /// <summary>
        /// Register services
        /// </summary>
        public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration config)
        {
            // Caching: a non distributed in memory implementation
            services.AddMemoryCache();
            // Caching: response caching services, replaced by EasyCaching.ResponseCaching
            //services.AddResponseCaching();


            // EasyCaching  https://easycaching.readthedocs.io
            // Inject Controller(IEasyCachingProvider provider) or Controller(IEasyCachingProviderFactory factory)
            // Configuration Section in appsettings.json
            /*
              "easycaching": {
                "inmemory": {
                  "MaxRdSecond": 120,
                  "EnableLogging": false,
                  "LockMs": 5000,
                  "SleepMs": 300,
                  "DBConfig": {
                    "SizeLimit": 10000,
                    "ExpirationScanFrequency": 60,
                    "EnableReadDeepClone": true,
                    "EnableWriteDeepClone": false
                  }
                },
                "redis": {
                  "MaxRdSecond": 120,
                  "EnableLogging": false,
                  "LockMs": 5000,
                  "SleepMs": 300,
                  "DBConfig": {
                    "Password": null,
                    "IsSsl": false,
                    "SslHost": null,
                    "ConnectionTimeout": 5000,
                    "AllowAdmin": true,
                    "Endpoints": [
                      {
                        "Host": "localhost",
                        "Port": 6379
                      }
                    ],
                    "Database": 0
                  }
                },
                "litedb": {
                  "MaxRdSecond": 120,
                  "EnableLogging": false,
                  "LockMs": 5000,
                  "SleepMs": 300,
                  "DBConfig": {
                    "FilePath": "App_Data/cache.db",
                    "Password": null
                  }
                }
              }
            */

            // add response caching by EasyCaching.ResponseCaching
            services.AddEasyCachingResponseCaching(EasyCachingConstValue.DefaultInMemoryName);

            //1. In-Memory Caching  https://easycaching.readthedocs.io/en/latest/In-Memory/
            var section = config.GetSection("easycaching:inmemory");
            if (section.Exists()) services.AddEasyCaching(options => options.UseInMemory(config, EasyCachingConstValue.DefaultInMemoryName, section.Path));
            else services.AddEasyCaching(options => options.UseInMemory(EasyCachingConstValue.DefaultInMemoryName));

            //2. Redis Cache  https://easycaching.readthedocs.io/en/latest/Redis/
            section = config.GetSection("easycaching:redis");
            if (section.Exists()) services.AddEasyCaching(options => options.UseRedis(config, EasyCachingConstValue.DefaultRedisName, section.Path));
            //2.1 Redis manager init.
            if (section.Exists())
            {
                var dbConfig = config.GetSection(section.Path);
                var redisOptions = new EasyCaching.Redis.RedisOptions();
                dbConfig.Bind(redisOptions);
                Data.RedisManager.Init(redisOptions.DBConfig);
            }

            //3. LiteDB Cache  https://easycaching.readthedocs.io/en/latest/LiteDB/
            section = config.GetSection("easycaching:litedb");
            if (section.Exists()) services.AddEasyCaching(options => options.UseLiteDB(config, EasyCachingConstValue.DefaultLiteDBName, section.Path));

            //4. Caching Interceptor  https://easycaching.readthedocs.io/en/latest/AspectCore/
            //services.ConfigureAspectCoreInterceptor(options => options.CacheProviderName = EasyCachingConstValue.DefaultInMemoryName);

            return services;
        }

        /// <summary>
        /// Use Cache Middleware
        /// </summary>
        public static IApplicationBuilder UseCache(this IApplicationBuilder app)
        {
            // Caching
            //app.UseResponseCaching();

            // use response caching
            app.UseEasyCachingResponseCaching();

            return app;
        }
    }
}
