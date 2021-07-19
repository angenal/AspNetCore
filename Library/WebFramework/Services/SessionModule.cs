using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace WebFramework.Services
{
    /// <summary>
    /// Session Module
    /// </summary>
    public static class SessionModule
    {
        /// <summary>
        /// Register services
        /// </summary>
        public static IServiceCollection AddSession(this IServiceCollection services, IConfiguration config)
        {
            // Adds a default in-memory implementation of IDistributedCache
            services.AddDistributedMemoryCache();

            // Uncomment the following line to use the Microsoft SQL Server implementation of IDistributedCache.
            // Note that this would require setting up the session state database.
            //services.AddSqlServerCache(o =>
            //{
            //    o.ConnectionString = "Server=.;Database=ASPNET5SessionState;Trusted_Connection=True;";
            //    o.SchemaName = "dbo";
            //    o.TableName = "Sessions";
            //});

            // Uncomment the following line to use the Redis implementation of IDistributedCache.
            // This will override any previously registered IDistributedCache service.
            //services.AddDistributedRedisCache(o =>
            //{
            //    o.Configuration = "localhost";
            //    o.InstanceName = "SampleInstance";
            //});

            services.AddSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromMinutes(60);
                o.IOTimeout = TimeSpan.FromSeconds(10);
            });

            return services;
        }
    }
}
