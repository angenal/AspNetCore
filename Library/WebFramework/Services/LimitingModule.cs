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
            //services.Configure<IpRateLimitPolicies>(config.GetSection("IpRateLimitPolicies"));
            // configure client rate limiting middleware
            //services.Configure<ClientRateLimitOptions>(config.GetSection("ClientRateLimiting"));
            //services.Configure<ClientRateLimitPolicies>(config.GetSection("ClientRateLimitPolicies"));
            // register stores
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            // configure resolvers, counter key builders
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            return services;
        }

        /// <summary>
        /// Configure services
        /// </summary>
        public static IApplicationBuilder UseLimiting(this IApplicationBuilder app)
        {
            app.UseIpRateLimiting();
            //app.UseClientRateLimiting();

            return app;
        }
    }
}
