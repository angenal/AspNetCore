using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using WebCore.Documents;
using WebFramework.Authorization;
using WebInterface;

namespace WebFramework.Services
{
    /// <summary>
    /// Services: Crypto,Database,Compression,Caching,CORS,Authentication,Authorization,Swagger,i18n...
    /// </summary>
    public static class ServicesModule
    {
        /// <summary>
        /// Adds services for api controllers
        /// </summary>
        public static IMvcBuilder AddApiControllers(this IServiceCollection services)
        {
            // 注册用户会话
            services.AddScoped<Session>();

            // 注册全局过滤器
            return services.AddControllers(options =>
            {
                // 全局异常过滤
                ExceptionHandlerModule.ApiExceptionsFilters(options);
                // 全局日志
                //options.Filters.Add<GlobalActionMonitor>();
                // 用户会话处理
                options.Filters.Add<AsyncSessionFilter>();
            });
        }

        /// <summary>
        /// Register all services
        /// </summary>
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env, IMvcBuilder builder)
        {
            // ApiServer (Kestrel,IIS) Module
            services.ConfigureServer(config, env);


            // App_Data Directory
            var dataDirectory = new DirectoryInfo(Path.Combine(env.ContentRootPath, "App_Data"));
            if (!dataDirectory.Exists) dataDirectory.Create();
            // Adds services required for using options
            services.AddOptions();

            // AutoMapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


            // Crypto services
            services.AddCrypto(config);


            // Database services
            services.AddDatabase(config);


            // Global Exception Handler
            services.AddExceptionHandler(ExceptionHandlerModule.ExceptionHandlerOptionsAction);
            // BadRequest Error Handler
            builder.ConfigureApiBehaviorOptions(ExceptionHandlerModule.ApiBehaviorOptionsAction);
            // Newtonsoft.Json override the default System.Text.Json of .NET Library
            builder.AddNewtonsoftJson(x =>
            {
                //x.SerializerSettings.ContractResolver = new DefaultContractResolver();
                x.SerializerSettings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
                x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                //x.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                //x.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });
            //builder.AddJsonOptions(x => x.JsonSerializerOptions.WriteIndented = true); // default System.Text.Json

            // Adds Controller's Attributes Injection for Mvc application
            //builder.AddControllersAsServices().AddViewComponentsAsServices().AddTagHelpersAsServices();

            // Data protection services to encrypt the stored user login information
            //services.AddDataProtection()
            //    .DisableAutomaticKeyGeneration()
            //    .PersistKeysToFileSystem(dataDirectory)
            //    .SetDefaultKeyLifetime(TimeSpan.FromDays(30))
            //    .SetApplicationName(env.ApplicationName);


            // Compression: response compression services
            services.AddCompression(config);

            // Caching services
            services.AddCache(config);

            // CORS services
            var allowedHosts = config.GetSection("AllowedHosts").Exists() ? config.GetSection("AllowedHosts").Value.Split(',') : new[] { "*" };
            services.AddCors(options => options.AddDefaultPolicy(policy =>
            {
                if (allowedHosts.Any(c => c == "*")) policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                else policy.WithOrigins(allowedHosts).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
            }));
            //services.AddCors(options => options.AddDefaultPolicy(policy => (allowedHosts.Any(c => c == "*") ? policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader() : policy.WithOrigins(allowedHosts).AllowAnyMethod().AllowAnyHeader().AllowCredentials()).Build()));
            services.AddHttpContextAccessor();


            // ApiAuthorization using WebFramework.Authorization
            services.AddApiAuthorization(config);
            // Microsoft.AspNetCore.Identity system for the specified User and Role types
            services.AddIdentityLiteDB(config);
            // Authentication with JWT
            services.AddJwtAuthentication(config);
            // Authentication with OAuth
            if (config.GetSection("OAuth").Exists())
            {
                var oAuth = services.AddAuthentication();
                string qq = config.GetValue<string>("OAuth:QQ:ClientId"), qqSecret = config.GetValue<string>("OAuth:QQ:ClientSecret");
                string wx = config.GetValue<string>("OAuth:Weixin:ClientId"), wxSecret = config.GetValue<string>("OAuth:Weixin:ClientSecret");
                if (!string.IsNullOrEmpty(qq) && !string.IsNullOrEmpty(qqSecret)) oAuth.AddQQAuthentication(t =>
                {
                    t.ClientId = qq;
                    t.ClientSecret = qqSecret;
                });
                if (!string.IsNullOrEmpty(wx) && !string.IsNullOrEmpty(wxSecret)) oAuth.AddWeixinAuthentication(t =>
                {
                    t.ClientId = wx;
                    t.ClientSecret = wxSecret;
                });
            }
            // Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("test", policy => policy.RequireClaim("name", "测试"));
                options.AddPolicy("User", policy => policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "role" && c.Value.StartsWith("User")) ||
                    context.User.HasClaim(c => c.Type == "name" && c.Value.StartsWith("User"))));
            });


            // ApiVersioning
            services.AddApiVersionService(config);
            // Swagger Document Generator For Development Environment
            if (env.IsDevelopment()) services.AddSwaggerGen(config);



            // configure ip rate limiting middleware
            services.AddLimiting(config);


            // Register i18n supports multi language
            services.RegisterResources(config);


            // SignalR
            services.AddSignalR();


            // Upload File
            services.AddUpload(config, env);

            // Email
            services.AddEmail(config);

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
                app.UseDeveloperExceptionPage();

                // Swagger Doc Generator
                app.UseSwaggerGen(config);
            }
            else
            {
                //app.UseExceptionHandler("/Error");
                app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts
                app.UseHttpsRedirection();
            }

            //app.UseHttpsRedirection();
            //app.UseFileServer(enableDirectoryBrowsing: true);

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = ctx => ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=604800") });
            //var ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
            app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All });

            // Use Compression
            app.UseCompression();
            // Use Caching
            app.UseCache();
            // Use CORS with Default Policy
            app.UseCors();

            // Use ip rate limiting middleware
            app.UseLimiting();

            // Use i18n supports multi language
            app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);

            // Use the given route template
            app.UseRouting();
            // Use Authentication with JWT or Microsoft.AspNetCore.Identity system
            app.UseAuthentication();
            // Use ApiAuthorization using WebFramework.Authorization
            app.UseApiAuthorization();
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
