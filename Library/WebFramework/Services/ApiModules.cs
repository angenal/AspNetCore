using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using WebCore;
using WebCore.Documents;
using WebFramework.Filters;
using WebFramework.Weixins;
using WebInterface;

namespace WebFramework.Services
{
    /// <summary>
    /// Services: Crypto,Database,Compression,Caching,CORS,Authentication,Authorization,Swagger,i18n...
    /// </summary>
    public static class ServicesModule
    {
        /// <summary>
        /// Provides programmatic configuration for the MVC framework.
        /// </summary>
        public sealed class Options
        {
            /// <summary>
            /// 注册全局过滤器 https://docs.microsoft.com/zh-cn/aspnet/core/mvc/controllers/filters
            /// </summary>
            internal static void MvcOptions(MvcOptions options)
            {
                // 全局日志记录
                options.Filters.Add<AsyncTraceMonitorFilter>();
                // 用户会话状态 user session
                options.Filters.Add<AsyncSessionFilter>();
                // 请求参数验证 启用 FluentValidation
                if (AsyncRequestValidationFilter.FluentValidation)
                {
                    options.Filters.Add<AsyncRequestValidationFilter>();
                    options.EnableEndpointRouting = false;
                }
                // 全局异常输出 output HttpResponseException
                //options.Filters.Add<HttpResponseExceptionFilter>();
            }
        }

        /// <summary>
        /// Adds services for api controllers
        /// </summary>
        public static IMvcBuilder AddApiControllers(this IServiceCollection services)
        {
            // 注册用户会话
            services.AddScoped<Session>();
            // 注册控制器
            return services.AddControllers(Options.MvcOptions);
        }

        /// <summary>
        /// Register all services
        /// </summary>
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env, IMvcBuilder builder)
        {
            // ApiServer (Kestrel,IIS)
            services.AddApiServer(config, env);

            // HTTP proxies forward information
            services.Configure<ForwardedHeadersOptions>(x =>
            {
                x.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                x.ForwardLimit = null; x.KnownNetworks.Clear(); x.KnownProxies.Clear(); //使用负载均衡或CDN时需清空限制
            });


            // App Data Directory
            var dataPath = Path.Combine(env.ContentRootPath, "data"); // Linux system file directories are case sensitive
            if (!Directory.Exists(dataPath)) dataPath = Path.Combine(env.ContentRootPath, "Data");
            if (!Directory.Exists(dataPath)) dataPath = Path.Combine(env.ContentRootPath, "App_Data");
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            var filePath = Path.Combine(env.WebRootPath, "file");
            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
            var tempPath = Path.Combine(env.WebRootPath, "temp");
            if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
            // Adds services required for using options
            services.AddOptions();


            // Adds i18n supports multi language
            services.AddResources(config, builder);


            // AutoMapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssembliesIsSubclassOf<AutoMapper.Profile>());


            // WebCore services
            services.AddCore(config);

            // Crypto services
            services.AddCrypto(config);


            // Database services
            services.AddDatabase(config);


            // Exception Handler services
            services.AddExceptionHandler(config, env);

            // Input Validation services
            services.AddValidation(builder);


            // Newtonsoft.Json override the default System.Text.Json of .NET Library
            builder.AddNewtonsoftJson(x =>
            {
                x.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver(); // 驼峰命名(首字母小写)
                x.SerializerSettings.DateFormatString = DefaultFormat.DateTime; // 输出时间格式为"yyyy-MM-dd HH:mm:ss"
                x.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                x.SerializerSettings.Converters.Add(new StringEnumConverter());
                //x.SerializerSettings.Converters.Add(new IsoDateTimeConverter());
                //x.SerializerSettings.Converters.Add(new JavaScriptDateTimeConverter());
                //x.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; // 不输出值为空的对象属性
            });
            //builder.AddJsonOptions(x => x.JsonSerializerOptions.WriteIndented = true); // default System.Text.Json

            // Adds Controller's Attributes Injection for Mvc application
            //builder.AddControllersAsServices().AddViewComponentsAsServices().AddTagHelpersAsServices();

            // Adds Microsoft.EntityFrameworkCore Data protection services to encrypt the stored user information
            //services.AddDataProtection()
            //    .DisableAutomaticKeyGeneration()
            //    .PersistKeysToFileSystem(dataDirectory)
            //    .SetDefaultKeyLifetime(TimeSpan.FromDays(30))
            //    .SetApplicationName(env.ApplicationName);

            // Adds health checks services
            services.AddHealthChecks();

            // Compression: response compression services
            services.AddCompression(config);

            // Caching services
            services.AddCache(config);

            // CORS services
            var allowedHosts = config.GetSection("AllowedHosts").Exists() ? config.GetSection("AllowedHosts").Value.Split(',') : new[] { "*" };
            services.AddCors(x => x.AddDefaultPolicy(p =>
            {
                if (allowedHosts.Any(c => c == "*")) p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(_ => true);
                else p.WithOrigins(allowedHosts).AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed(_ => true);
            }));
            // Adds HttpContext for Controllers
            services.AddHttpContextAccessor();


            // Configure ip rate limiting middleware
            services.AddLimiting(config);


            // Authentication + Authorization
            services.AddAuth(config, env);


            // ApiVersioning
            services.AddApiVersionService(config);
            // Swagger Document Generator For Development Environment
            if (env.IsDevelopment()) services.AddSwaggerGen(config);



            // SignalR  https://docs.microsoft.com/zh-cn/aspnet/core/signalr
            services.AddSignalR();
            // SignalR 提升传输性能 Microsoft.AspNetCore.SignalR.Protocols.MessagePack
            //services.AddSignalR().AddMessagePackProtocol();


            // Weixin  https://github.com/JeffreySu/WeiXinMPSDK
            services.AddWeixin(config, env);


            // Upload File
            services.AddUpload(config, env);

            // Email
            services.AddEmail(config);

            // Image
            services.AddSingleton<IImageTools, ImageTools>();

            // Excel
            services.AddSingleton<IExcelTools, ExcelTools>();
            // Word
            services.AddSingleton<IWordTools, WordTools>();
            // PPT
            services.AddSingleton<IPptTools, PptTools>();
            // PDF
            services.AddSingleton<IPdfTools, PdfTools>();


            // Hangfire: Background jobs and workers
            services.AddHangfire(config);
            // BackgroundService: TaskService
            services.AddHostedService<TaskService>();
            // FluentScheduler: TaskManager
            services.AddSingleton<ITaskManager, TaskManager>(_ => TaskManager.Default);
            // Allow to raise a task completion source with minimal costs and attempt to avoid stalls due to thread pool starvation.
            services.AddSingleton<ITaskExecutor, TaskExecutor>(_ => TaskExecutor.Default);


            // other services

            return services;
        }

        /// <summary>
        /// Configure all services
        /// </summary>
        public static IApplicationBuilder Configure(this IApplicationBuilder app, IConfiguration config, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            //app.UseBlockingDetection(); // using Ben.Diagnostics;

            // Run Development Environment
            if (env.IsDevelopment())
            {
                // Captures synchronous and asynchronous System.Exception instances from the pipeline and generates HTML error responses.
                //app.UseDeveloperExceptionPage();
                app.UseExceptionHandler();

                // Swagger Doc Generator
                app.UseSwaggerGen(config);
            }
            else
            {
                app.UseExceptionHandler();
                app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts
            }

            app.UseHttpsRedirection();

            // Use ApiServer (Kestrel,IIS)
            app.UseApiServer(config);

            // HTTP proxies forward information
            app.UseForwardedHeaders().UseCertificateForwarding();

            //app.UseFileServer(enableDirectoryBrowsing: true);
            app.UseDefaultFiles(new DefaultFilesOptions() { DefaultFileNames = new[] { "index.html" } });
            app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = c => c.Context.Response.Headers.Append("Cache-Control", "public, max-age=604800") });
            //app.Map("/assets", x => x.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "assets")) }));

            // Use Compression
            app.UseCompression();
            // Use Caching
            app.UseCache();
            // Use CORS with Default Policy
            app.UseCors();

            // Use ip rate limiting middleware
            app.UseLimiting();

            // Use i18n supports multi language
            app.UseResources();

            // Use the given route template
            app.UseRouting();

            // Use Weixin before authorization
            app.UseWeixin(env);

            // Use Authentication with JWT or Microsoft.AspNetCore.Identity system
            app.UseAuthentication();
            // Use ApiAuthorization using WebFramework.Authorization
            //app.UseApiAuthorization();
            // Use Authorization
            app.UseAuthorization();

            // Logging: Serilog Logging Module
            app.UseSerilogLogging(config, env, loggerFactory);

            // Hangfire Dashboard
            app.UseHangfire(config);

            return app;
        }
    }
}
