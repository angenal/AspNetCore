using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebCore;
using WebFramework.Services;

namespace WebFramework
{
    public class Startup
    {
        readonly IConfiguration Configuration;
        readonly IWebHostEnvironment Environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public static void Run<TStartup>(string[] args) where TStartup : class
        {
            // 系统入口:初始化
            Main.Init();
            // 系统应用:初始化
            var host = Host.CreateDefaultBuilder(args)
                //.ConfigureAppConfiguration((context, builder) =>
                //{
                //    //The following configuration has been loaded automatically by default
                //    builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true);
                //    builder.AddEnvironmentVariables();//builder.AddEnvironmentVariables("ASPNETCORE_");
                //    builder.AddCommandLine(args);
                //})
                .ConfigureLogging()
                .ConfigureWebHostDefaults(builder => builder.UseStartup<TStartup>().UseSentryMonitor())
                .Build();

            host.Run();
        }

        public void ConfigServices(IServiceCollection services)
        {
            // Adds services for api controllers
            var builder = services.AddApiControllers(); // services.AddMvc() for Mvc application

            // Services: Crypto,Database,Compression,Caching,CORS,Authentication,Authorization,Swagger,i18n...
            services.ConfigureServices(Configuration, Environment, builder);
        }

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

        /// <summary>
        /// ApiController Endpoints Maps
        /// </summary>
        /// <param name="endpoints"></param>
        public void UseEndpointsMaps(IEndpointRouteBuilder endpoints)
        {
            // 下载文件 Tus Endpoint Handler
            endpoints.MapGet("/" + TusFileServer.UrlPath.Trim('/') + "/{fileId}", TusFileServer.DownloadHandler);
        }
    }
}
