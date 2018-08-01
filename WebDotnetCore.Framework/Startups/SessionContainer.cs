using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebFramework.Startups
{
    public class SessionContainer
    {

        public void ConfigureServices(IServiceCollection services)
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
                o.IdleTimeout = TimeSpan.FromSeconds(120);
            });
        }
        public void Configure(IApplicationBuilder app)
        {
            app.UseSession();
        }
    }
}
