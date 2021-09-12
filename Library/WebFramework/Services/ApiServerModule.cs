using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebInterface.Settings;

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
            var section = config.GetSection(ApiSettings.AppSettings);
            if (!section.Exists()) return services;

            // Configures ApiSettings
            if (ApiSettings.Instance == null)
            {
                ApiSettings.Instance = new ApiSettings();
                // Register IOptions<ApiSettings> from appsettings.json
                services.Configure<ApiSettings>(section);
                config.Bind(ApiSettings.AppSettings, ApiSettings.Instance);
            }

            // Configures Limit Options
            int maxLengthLimit = ApiSettings.Instance.MaxLengthLimit; // 提交元素个数限制
            int maxRequestBodySize = ApiSettings.Instance.MaxRequestBodySize; // 提交数据文本字节数量限制
            int maxMultipartBodySize = ApiSettings.Instance.MaxMultipartBodySize; // 上传文件大小限制

            //services.Configure<IISOptions>(opt => { }); // Configure IIS Out-Of-Process.

            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = maxRequestBodySize;
                options.AllowSynchronousIO = true; // 启用同步IO
            });
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = maxRequestBodySize;
                options.AllowSynchronousIO = true; // 启用同步IO
            });

            services.Configure<FormOptions>(options =>
            {
                options.KeyLengthLimit = maxLengthLimit;
                options.ValueCountLimit = maxLengthLimit;
                options.ValueLengthLimit = maxMultipartBodySize;
                options.BufferBodyLengthLimit = FormOptions.DefaultBufferBodyLengthLimit;
                options.MemoryBufferThreshold = FormOptions.DefaultMemoryBufferThreshold;
                options.MultipartHeadersCountLimit = maxLengthLimit;
                options.MultipartHeadersLengthLimit = maxRequestBodySize;
                options.MultipartBoundaryLengthLimit = maxLengthLimit;
                options.MultipartBodyLengthLimit = maxMultipartBodySize;
            });

            // 系统性能指标的跟踪监控 App.Metrics.AspNetCore  https://www.youtube.com/watch?v=sM7D8biBf4k
            services.AddMetrics();

            return services;
        }
    }
}
