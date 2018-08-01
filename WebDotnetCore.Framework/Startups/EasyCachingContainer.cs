using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyCaching.Core;
using EasyCaching.InMemory;
using EasyCaching.Redis;
using EasyCaching.Memcached;
using EasyCaching.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace WebFramework.Startups
{
    /// <summary>
    /// EasyCaching Startup And Inject IEasyCachingProvider
    /// </summary>
    public class EasyCachingContainer
    {
        private static object _lock = new object();

        public EasyCachingContainer(IConfiguration configuration)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //1. In-Memory Cache
            services.AddDefaultInMemoryCache();

            //2. Redis Cache
            //services.AddDefaultRedisCache(option=>
            //{                
            //    option.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            //    option.Password = "";
            //});

            //3. Memcached Cache
            //services.AddDefaultMemcached(option=>
            //{                
            //    option.AddServer("127.0.0.1",11211);        
            //});

            //4. SQLite Cache
            //services.AddSQLiteCache(option=>{});
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //3. Memcache Cache
            //app.UseDefaultMemcached();

            //4. SQLite Cache
            //app.UseSQLiteCache();
        }
    }
}
