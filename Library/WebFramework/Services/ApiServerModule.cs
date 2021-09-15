using App.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
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
        public static IServiceCollection AddApiServer(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
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

            // Configures Limit on Request
            int maxLengthLimit = ApiSettings.Instance.MaxLengthLimit; // 提交元素个数限制 (默认 8000)
            int maxRequestBodySize = ApiSettings.Instance.MaxRequestBodySize; // 提交数据文本字节数量限制 (默认 28.6 MB)
            int maxMultipartBodySize = ApiSettings.Instance.MaxMultipartBodySize; // 上传文件大小限制 (默认 128 MB)

            //services.Configure<IISOptions>(opt => { }); // Configure IIS Out-Of-Process.

            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = Math.Max(maxRequestBodySize, maxMultipartBodySize);
                options.AllowSynchronousIO = true; // 启用同步IO
            });
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = Math.Max(maxRequestBodySize, maxMultipartBodySize);
                options.AllowSynchronousIO = true; // 启用同步IO
            });
            services.Configure<FormOptions>(options =>
            {
                options.KeyLengthLimit = maxLengthLimit;
                options.ValueCountLimit = maxLengthLimit;
                options.ValueLengthLimit = maxRequestBodySize;
                options.BufferBodyLengthLimit = FormOptions.DefaultBufferBodyLengthLimit;
                options.MemoryBufferThreshold = FormOptions.DefaultMemoryBufferThreshold;
                options.MultipartHeadersCountLimit = maxLengthLimit;
                options.MultipartHeadersLengthLimit = maxRequestBodySize;
                options.MultipartBoundaryLengthLimit = maxRequestBodySize;
                options.MultipartBodyLengthLimit = maxMultipartBodySize;
            });

            // 系统性能指标的跟踪监控 App.Metrics.AspNetCore
            // 参考Grafana 数据源  http://localhost:3000/datasources
            /* appsettings.json
              "AppMetrics": {
                "Open": true,
                "Name": "ApiDemo.NET5",
                "BaseUri": "http://127.0.0.1:8086",
                "Database": "ApiDemo_NET5_Metrics",
                "UserName": "admin",
                "Password": "HGJ766GR767FKJU0"
              }
            */

            section = config.GetSection("AppMetrics");
            if (!section.Exists() || !section.GetSection("BaseUri").Exists() || !section.GetSection("Database").Exists() || !section.GetSection("UserName").Exists() || !section.GetSection("Password").Exists()) return services;
            if (section.GetSection("Open").Exists() && !section.GetSection("Open").Get<bool>()) return services;
            services.AddMetrics(AppMetrics.CreateDefaultBuilder().Configuration.Configure(options =>
            {
                options.AddAppTag(section.GetSection("Name").Exists() ? section.GetSection("Name").Value : env.ApplicationName);
                options.AddEnvTag(section.GetSection("Env").Exists() ? section.GetSection("Env").Value : env.EnvironmentName);
            }).Report.ToInfluxDb(options =>
            {
                options.InfluxDb.BaseUri = new Uri(section.GetSection("BaseUri").Value);
                options.InfluxDb.Database = section.GetSection("Database").Value;
                options.InfluxDb.UserName = section.GetSection("UserName").Value;
                options.InfluxDb.Password = section.GetSection("Password").Value;
                options.FlushInterval = TimeSpan.FromSeconds(5);
                options.HttpPolicy.BackoffPeriod = TimeSpan.FromSeconds(30);
                options.HttpPolicy.Timeout = TimeSpan.FromSeconds(10);
                options.HttpPolicy.FailuresBeforeBackoff = 5;
            }).Build());
            services.AddMetricsEndpoints();
            services.AddMetricsTrackingMiddleware();
            services.AddMetricsReportingHostedService();

            return services;
        }

        /// <summary>
        /// Configure services
        /// </summary>
        public static IApplicationBuilder UseApiServer(this IApplicationBuilder app, IConfiguration config)
        {
            if (config.GetSection("AppMetrics").Exists() && app.ApplicationServices.GetService<IMetricsRoot>() != null)
            {
                // 参考Grafana Import Dashboard - App Metrics - Web Monitoring - InfluxDB https://grafana.com/grafana/dashboards/2125
                app.UseMetricsAllEndpoints();
                app.UseMetricsAllMiddleware();

                //app.UseMetricsEndpoint();
                //app.UseMetricsTextEndpoint();
                //app.UseMetricsActiveRequestMiddleware();
                //app.UseMetricsApdexTrackingMiddleware();
                //app.UseMetricsErrorTrackingMiddleware();
                //app.UseMetricsOAuth2TrackingMiddleware();
                //app.UseMetricsPostAndPutSizeTrackingMiddleware();
                //app.UseMetricsRequestTrackingMiddleware();
            }

            return app;
        }
    }
}
