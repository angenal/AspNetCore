using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebFramework.Services
{
    /// <summary>
    /// Api Server(Kestrel,IIS) Module
    /// </summary>
    public static class ApiServerModule
    {
        /// <summary>
        /// Register all services
        /// </summary>
        public static IServiceCollection ConfigureServer(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            //services.Configure<IISOptions>(opt => { }); // Configure IIS Out-Of-Process.

            services.Configure<IISServerOptions>(options => options.MaxRequestBodySize = 30000000); // Defaults to 30,000,000 bytes (~28.6 MB)
            services.Configure<KestrelServerOptions>(options => options.Limits.MaxRequestBodySize = 30000000);

            services.Configure<FormOptions>(options =>
            {
                options.KeyLengthLimit = 128;
                options.ValueLengthLimit = 8000;
                options.ValueCountLimit = 1024;
                options.MultipartHeadersCountLimit = 1024;
                options.MultipartHeadersLengthLimit = 8000;
                options.MemoryBufferThreshold = FormOptions.DefaultMemoryBufferThreshold;
                options.BufferBodyLengthLimit = FormOptions.DefaultBufferBodyLengthLimit;
                options.MultipartBoundaryLengthLimit = FormOptions.DefaultMultipartBoundaryLengthLimit;
                options.MultipartBodyLengthLimit = FormOptions.DefaultMultipartBodyLengthLimit;
            });

            return services;
        }
    }
}
