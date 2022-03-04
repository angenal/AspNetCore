using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebFramework.Services
{
    /// <summary>
    /// Limiting services module
    /// IpRateLimiting: https://github.com/stefanprodan/AspNetCoreRateLimit/wiki/IpRateLimitMiddleware
    /// </summary>
    public static class LimitingModule
    {
        /// <summary>
        /// Register services
        /// </summary>
        public static IServiceCollection AddLimiting(this IServiceCollection services, IConfiguration config)
        {
            var section = config.GetSection("IpRateLimiting");
            if (!section.Exists()) return services;

            services.Configure<IpRateLimitOptions>(section);
            // register stores
            services.AddInMemoryRateLimiting();
            // configure resolvers, counter key builders
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            return services;
        }

        /// <summary>
        /// Configure services
        /// </summary>
        public static IApplicationBuilder UseLimiting(this IApplicationBuilder app, IConfiguration config)
        {
            var section = config.GetSection("IpRateLimiting");
            if (!section.Exists()) return app;

            app.UseIpRateLimiting();

            return app;
        }
    }
}
