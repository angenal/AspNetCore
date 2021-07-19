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

            // EasyCaching  https://easycaching.readthedocs.io
            // Inject Controller(IEasyCachingProvider provider) or Controller(IEasyCachingProviderFactory factory)
            // Configuration Section in appsettings.json
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
                },
            "LiteDB": "Filename=App_Data/Hangfire.db;Password=HGJ766GR767FKJU0",
            */

            //1. In-Memory Caching  https://easycaching.readthedocs.io/en/latest/In-Memory/
            var section = config.GetSection("easycaching:inmemory");
            if (section.Exists()) services.AddEasyCaching(options => options.UseInMemory(config, EasyCachingDataSource.Memory, section.Path));

            //2. Redis Cache  https://easycaching.readthedocs.io/en/latest/Redis/
            section = config.GetSection("easycaching:redis");
            if (section.Exists()) services.AddEasyCaching(options => options.UseRedis(config, EasyCachingDataSource.Redis, section.Path));

            //3. LiteDB Cache  https://easycaching.readthedocs.io/en/latest/LiteDB/
            section = config.GetSection("easycaching:litedb");
            if (section.Exists()) services.AddEasyCaching(options => options.UseLiteDB(config, EasyCachingDataSource.LiteDB, section.Path));

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
