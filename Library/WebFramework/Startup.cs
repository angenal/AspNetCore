using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebCore;
using WebFramework.Services;
using WebFramework.SignalR;

namespace WebFramework
{
    /// <summary></summary>
    public class Startup
    {
        readonly IConfiguration Configuration;
        readonly IWebHostEnvironment Environment;

        /// <summary></summary>
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        /// <summary></summary>
        public static void Run<TStartup>(string[] args) where TStartup : Startup
        {
            // 系统入口:初始化
            Main.Init();
            // 系统应用:初始化
            var host = Host.CreateDefaultBuilder(args)
                //.ConfigureAppConfiguration((context, builder) => // Host.CreateDefaultBuilder:
                //{
                //    //The following configuration has been loaded automatically by default
                //    builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true);
                //    builder.AddEnvironmentVariables();//builder.AddEnvironmentVariables("ASPNETCORE_");
                //    builder.AddCommandLine(args);
                //})
                // 系统性能指标的跟踪监控  https://prometheus.io/download  https://grafana.com/grafana/download
                .UseMetricsWebTracking() // Tracking URL: /metrics /metrics-text
                .UseMetrics(options =>
                {
                    options.EndpointOptions = x =>
                    {
                        x.EnvironmentInfoEndpointEnabled = true;
                        x.MetricsEndpointOutputFormatter = new MetricsPrometheusProtobufOutputFormatter();
                        x.MetricsTextEndpointOutputFormatter = new MetricsPrometheusTextOutputFormatter();
                    };
                })
                .ConfigureLogging()
                .ConfigureWebHostDefaults(builder => builder.UseStartup<TStartup>().UseSentryMonitor())
                .Build();

            host.Run();
        }

        /// <summary></summary>
        public void ConfigServices(IServiceCollection services)
        {
            // Adds services for api controllers
            var builder = services.AddApiControllers(); // services.AddMvc() for Mvc application

            // Services: Crypto,Database,Compression,Caching,CORS,Authentication,Authorization,Swagger,i18n...
            services.ConfigureServices(Configuration, Environment, builder);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        /// <summary></summary>
        public void ConfigApp(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            // Services: Crypto,Database,Compression,Caching,CORS,Authentication,Authorization,Swagger,i18n...
            app.Configure(Configuration, env, loggerFactory);

            //app.UseAbp();
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("/", async context =>
            //    {
            //        await context.Response.WriteAsync("Hello World!");
            //    });
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>ApiController Endpoints Maps</summary>
        public void UseEndpointsMaps(IEndpointRouteBuilder endpoints)
        {
            // 聊天系统 ChatHub
            endpoints.MapHub<ChatHub>("/chat", options => options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling);

            // 下载文件 Tus Endpoint Handler
            endpoints.MapGet("/" + TusFileServer.UrlPath.Trim('/') + "/{fileId}", TusFileServer.DownloadHandler);

            // 查询异常日志记录
            endpoints.MapGet(ExceptionLogService.QueryUrl, ExceptionLogService.QueryHandler);
            endpoints.MapDelete(ExceptionLogService.DeleteUrl, ExceptionLogService.DeleteHandler);

            // 默认路由 Default MVC with culture
            endpoints.MapControllerRoute("default", "{culture:culture}/{controller=Home}/{action=Index}/{id?}");
        }
    }
}
