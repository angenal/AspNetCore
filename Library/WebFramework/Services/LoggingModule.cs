using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Linq;
using System.Text;
using WebFramework.Data;

namespace WebFramework.Services
{
    /* appsettings.json
      "Logging": {
        "LogLevel": {
          "Default": "Warning",
          "Microsoft": "Error",
          "Microsoft.Hosting.Lifetime": "Error",
          "System": "Error"
        },
        "LogManage": {
          "Path": "logs",
          "User": "demo",
          "Pass": "demo"
        }
      },
      "Serilog": {
        "MinimumLevel": {
          "Default": "Debug",
          "Override": {
            "Microsoft": "Information",
            "System": "Information"
          }
        },
        "WriteTo": [
          {
            "Name": "Console",
            "Args": {
              "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
              "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}"
            }
          },
          {
            "Name": "File",
            "Args": {
              "path": "logs/LogFiles/.log",
              "rollingInterval": "Day",
              "outputTemplate": "{Timestamp:HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
              "flushToDiskInterval": "00:00:05",
              "retainedFileCountLimit": 7,
              "buffered": true
            }
          }
        ],
        "Enrich": [ "FromLogContext", "WithMachineName", "WithProcessId", "WithProcessName", "WithThreadId" ]
      }
    */

    /// <summary></summary>
    public sealed class Logs
    {
        /// <summary>
        /// Use Default Logging for Development Environment
        /// </summary>
        internal static bool Enabled = false;
        /// <summary>
        /// Use Serilog Logging for Production Environment, Output Directory: /logs
        /// </summary>
        internal static bool EnabledSerilog = false;
        /// <summary>
        /// Use Serilog Logging Configuration with appsettings.json
        /// </summary>
        internal static bool EnabledSerilogConfiguration = false;
        /// <summary>
        /// Configuration Section in appsettings.json
        /// </summary>
        public const string AppSettings = "Logging";
        /// <summary>
        /// Configuration Exception Log Manage in appsettings.json
        /// </summary>
        public const string AppSettingsLogManage = "Logging:LogManage";
        /// <summary>
        /// Configuration Serilog in appsettings.json
        /// </summary>
        public const string AppSettingSerilog = "Serilog";
        /// <summary>
        /// Log files root directory
        /// </summary>
        public static string RootPath = "logs";
        /// <summary>
        /// Log Manage Configuration
        /// </summary>
        public static LogManage WebManage = new LogManage { Path = RootPath };
        /// <summary>
        /// Asynchronous record log file
        /// </summary>
        public static AsyncExceptionHandler<ExceptionLog> ExceptionHandler;
        ///// <summary>
        ///// Web logs directory for status 500
        ///// </summary>
        //static string StatusDir500 = StatusCodes.Status500InternalServerError.ToString();
        ///// <summary>
        ///// Web logs record status 500
        ///// </summary>
        //static bool StatusDir500Exists = false;
        /// <summary>
        /// Log Init
        /// </summary>
        internal static void Init()
        {
            // 启用Logging for any environment
            Enabled = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

            // 启用Serilog 请提前创建日志目录logs
            EnabledSerilog = Directory.Exists(RootPath);
        }
    }

    /// <summary>
    /// Logging Module
    /// </summary>
    public static class LoggingModule
    {
        /// <summary>
        /// Create default logger, it can be replace with UseSerilog() in IHostBuilder
        /// </summary>
        public static void CreateBootstrapLogger() => Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        /// <summary>
        /// Use new LoggerConfiguration for CreateLogger
        /// https://github.com/serilog/serilog-settings-configuration/blob/dev/sample/Sample
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static LoggerConfiguration NewLoggerConfiguration(string path = "appsettings-serilog.json") => new LoggerConfiguration()
            .ReadFrom.Configuration(new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(path, optional: false, reloadOnChange: true)
            .Build());

        /// <summary>
        /// Configure Logging Module
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureLogging(this IHostBuilder builder)
        {
            Logs.Init();
            if (!Logs.Enabled && !Logs.EnabledSerilog)
            {
                CreateBootstrapLogger(); // If not enabled, Use default console logging for development
                return builder;
            }

            // Use Default Logging, Only Output Console
            builder.ConfigureLogging((context, builder) =>
            {
                builder.ClearProviders();
                var configuration = context.Configuration.GetSection(Logs.AppSettings);
                if (configuration.Exists())
                {
                    builder.AddConfiguration(configuration).AddConsole(); // using Microsoft.Extensions.Logging;
#if DEBUG
                    builder.AddDebug();
#endif
                }
                else CreateBootstrapLogger(); // using Default Bootstrap Logger of Serilog
            });

            // If Not Use Serilog
            if (!Logs.EnabledSerilog) return builder;

            // If Not Use Serilog Logging Configuration with appsettings.json
            if (!Logs.EnabledSerilogConfiguration) return builder.UseSerilog();

            // Use Serilog appsettings.json (replace default logger) https://github.com/serilog/serilog-settings-configuration
            return builder.UseSerilog((context, services, configuration) => configuration
                  .ReadFrom.Configuration(context.Configuration, Logs.AppSettingSerilog)
                  .ReadFrom.Services(services)//.Enrich.WithThreadId() // using Serilog.Enrichers.Thread
                  .Enrich.FromLogContext());
        }

        /// <summary>
        /// Configure Serilog Logging Module
        /// </summary>
        public static IApplicationBuilder UseSerilogLogging(this IApplicationBuilder app, IConfiguration config, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (!Logs.EnabledSerilog) return app;

            // Add Serilog to the logging pipeline.
            loggerFactory.AddSerilog();

            // Input LogContext
            app.Use(async (httpContext, next) =>
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString();
                Serilog.Context.LogContext.PushProperty("IP", string.IsNullOrEmpty(ip) ? "?" : ip);
                Serilog.Context.LogContext.PushProperty("User", httpContext.User.Identity.IsAuthenticated ? httpContext.User.Identity.Name : "?");
                await next.Invoke();
            });

            // If Use Serilog Logging Configuration with appsettings.json
            if (Logs.EnabledSerilogConfiguration) return app;

            // Output to File
            var dir = new DirectoryInfo(Path.Combine(Logs.RootPath, "file"));
            if (!dir.Exists) dir.Create();

            // Output Template
            var template = "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

            // Create default logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                //.Enrich.WithThreadId() // using Serilog.Enrichers.Thread
                // Input LogContext
                .Enrich.FromLogContext()
                // Output to Console
                .WriteTo.Console()
                // Output to File
                .WriteTo.File(dir.FullName, restrictedToMinimumLevel: LogEventLevel.Warning, outputTemplate: template, buffered: true, shared: false, flushToDiskInterval: TimeSpan.FromSeconds(5), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7, encoding: Encoding.UTF8)
                // Output to RavenDB
                .WriteTo.RavenDB(RavenDb.CreateRavenDocStore(env), errorExpiration: TimeSpan.FromDays(60))
                .CreateLogger();

            // Adds middleware for streamlined request logging
            // messageTemplate: "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms"
            //app.UseSerilogRequestLogging();

            return app;
        }
    }

    /// <summary></summary>
    public partial class LogManage
    {
        /// <summary>the directory in web root path</summary>
        public string Path { get; set; }
        /// <summary></summary>
        public string User { get; set; }
        /// <summary></summary>
        public string Pass { get; set; }
    }
}
