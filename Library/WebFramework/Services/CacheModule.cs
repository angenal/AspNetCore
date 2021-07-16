using Microsoft.AspNetCore.Builder;
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
        public static IServiceCollection AddEasyCaching(this IServiceCollection services)
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

            return services;
        }

        public static IApplicationBuilder UseEasyCaching(this IApplicationBuilder app)
        {
            //3. Memcache Cache
            //app.UseDefaultMemcached();

            //4. SQLite Cache
            //app.UseSQLiteCache();

            return app;
        }
    }
}
