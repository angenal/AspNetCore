using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
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
        }
      },
      "Serilog": {
        "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
        "MinimumLevel": "Information",
        "WriteTo": [
          { "Name": "Console" },
          {
            "Name": "File",
            "Args": {
              "buffered": true,
              "flushToDiskInterval": "5s",
              "path": "logs/file/log-.txt",
              "outputTemplate": "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
              "rollingInterval": "Day",
              "retainedFileCountLimit": "7",
              "restrictedToMinimumLevel": "Warning"
            }
          }
        ],
        "Enrich": [ "FromLogContext", "WithThreadId" ],
        "Destructure": [],
        "Properties": {}
      },
    */

    /// <summary>
    /// Logging Module
    /// </summary>
    public static class LoggingModule
    {
        /// <summary>
        /// Use Default Logging for Development Environment
        /// </summary>
        static bool Enabled = false;
        /// <summary>
        /// Use Serilog Logging for Production Environment, Output Directory: /logs
        /// </summary>
        static bool EnabledSerilog = false;
        /// <summary>
        /// Use Serilog Logging Configuration with appsettings.json
        /// </summary>
        static bool EnabledSerilogConfiguration = false;

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
            Enabled = "Development" == Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            EnabledSerilog = Directory.Exists("logs");
            if (!Enabled && !EnabledSerilog)
            {
                CreateBootstrapLogger(); // If not enabled, Use default console logging for development
                return builder;
            }

            // Use Default Logging, Only Output Console
            builder.ConfigureLogging((context, builder) =>
            {
                builder.ClearProviders();
                var configuration = context.Configuration.GetSection("Logging");
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
            if (!EnabledSerilog) return builder;

            // If Not Use Serilog Logging Configuration with appsettings.json
            if (!EnabledSerilogConfiguration) return builder.UseSerilog();

            // Use Serilog appsettings.json (replace default logger) https://github.com/serilog/serilog-settings-configuration
            return builder.UseSerilog((context, services, configuration) => configuration
                  .ReadFrom.Configuration(context.Configuration, "Serilog")
                  .ReadFrom.Services(services)//.Enrich.WithThreadId() // using Serilog.Enrichers.Thread
                  .Enrich.FromLogContext());
        }

        /// <summary>
        /// Configure Serilog Logging Module
        /// </summary>
        public static IApplicationBuilder UseSerilogLogging(this IApplicationBuilder app, IConfiguration config, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (!EnabledSerilog) return app;

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
            if (EnabledSerilogConfiguration) return app;

            // Output to File
            var dir = new DirectoryInfo("logs/file");
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
                .WriteTo.File(dir.FullName, outputTemplate: template, buffered: true, flushToDiskInterval: TimeSpan.FromSeconds(10), rollingInterval: RollingInterval.Day)
                // Output to RavenDB
                .WriteTo.RavenDB(RavenDb.CreateRavenDocStore(env), errorExpiration: TimeSpan.FromDays(60))
                .CreateLogger();

            // Adds middleware for streamlined request logging
            // messageTemplate: "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms"
            //app.UseSerilogRequestLogging();

            return app;
        }
    }
}
