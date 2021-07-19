using EasyCaching.LiteDB;
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
            //services.AddMemoryCache();
            // Caching: response caching services
            //services.AddResponseCaching();

            //EasyCaching  https://easycaching.readthedocs.io
            /*
              "inmemory": {
                "MaxRdSecond": 120,
                "EnableLogging": false,
                "LockMs": 5000,
                "SleepMs": 300,
                "DBConfig":{
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
                    "dbconfig": {
                        "Password": null,
                        "IsSsl": false,
                        "SslHost": null,
                        "ConnectionTimeout": 5000,
                        "AllowAdmin": true,
                        "Endpoints": [
                            {
                                "Host": "localhost",
                                "Port": 6739
                            }
                        ],
                        "Database": 0
                    }
                }
            */

            //1. In-Memory Caching  https://easycaching.readthedocs.io/en/latest/In-Memory/
            services.AddEasyCaching(options => options.UseInMemory(config, "DefaultInMemory", "easycaching:inmemory"));

            //2. Redis Cache  https://easycaching.readthedocs.io/en/latest/Redis/
            services.AddEasyCaching(options => options.UseRedis(config, "DefaultRedis", "easycaching:redis"));

            //3. LiteDB Cache
            services.AddEasyCaching(options => options.UseLiteDB(config => config.DBConfig = new LiteDBDBOptions { FileName = "s1.ldb" }));

            return services;
        }

        /// <summary>
        /// Use Cache Middleware
        /// </summary>
        public static IApplicationBuilder UseCache(this IApplicationBuilder app)
        {
            //3. Memcache Cache
            //app.UseDefaultMemcached();

            //4. SQLite Cache
            //app.UseSQLiteCache();

            return app;
        }
    }
}
