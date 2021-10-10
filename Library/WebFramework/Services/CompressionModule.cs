using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;
using System.Linq;

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
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
                options.Providers.Add<BrotliCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    "image/svg+xml",
                    "application/atom+xml",
                    "application/xhtml+xml",
                    "text/html; charset=utf-8",
                });
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
