using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;

namespace WebFramework.Services
{
    /// <summary>
    /// Compression Module
    /// </summary>
    public static class CompressionModule
    {
        /// <summary>
        /// Register services
        /// </summary>
        public static IServiceCollection AddCompression(this IServiceCollection services, IConfiguration config)
        {
            // Compression: response compression services
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
            services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
            services.AddResponseCompression(action =>
            {
                action.EnableForHttps = true;
                action.Providers.Add<GzipCompressionProvider>();
                action.Providers.Add<BrotliCompressionProvider>();
            });

            return services;
        }

        /// <summary>
        /// Use Compression Middleware
        /// </summary>
        public static IApplicationBuilder UseCompression(this IApplicationBuilder app)
        {
            // Use Compression
            app.UseResponseCompression();

            return app;
        }
    }
}
