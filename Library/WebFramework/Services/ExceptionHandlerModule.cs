using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebCore;
using WebFramework.Filters;

namespace WebFramework.Services
{
    /// <summary>
    /// Configure Global monitoring and exception handler module
    /// </summary>
    public static class ExceptionHandlerModule
    {
        //static readonly BackgroundJobClient JobClient = new BackgroundJobClient(new MemoryStorage(new MemoryStorageOptions()));
        //static readonly BackgroundJobServer JobServer = new BackgroundJobServer(new BackgroundJobServerOptions { ServerName = $"{LogsRootDir}-{Environment.ProcessId}" }, new MemoryStorage(new MemoryStorageOptions()));

        /// <summary>
        /// Init Exception Handler services
        /// </summary>
        public static void AddExceptionHandler(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            // Global Exception Log Manage with a URL: /logs <=> the directory in web root path
            var section = config.GetSection(Logs.AppSettingsLogManage);
            if (section.Exists())
            {
                if (section.GetSection("Trace").Exists()) Logs.Manage.Trace = section.GetValue<bool>("Trace");
                if (section.GetSection("Limit").Exists()) Logs.Manage.Limit = section.GetValue<int>("Limit");
                if (section.GetSection("Path").Exists()) Logs.Manage.Path = section.GetValue<string>("Path").Trim('/');
                if (section.GetSection("User").Exists()) Logs.Manage.User = section.GetValue<string>("User").Trim();
                if (section.GetSection("Pass").Exists()) Logs.Manage.Pass = section.GetValue<string>("Pass").Trim();
            }
            var path = Path.Combine(env.WebRootPath, Logs.Manage.Path);
            Logs.Enabled = Directory.Exists(path); // && File.Exists(Path.Combine(path, "index.html"));
            if (!Logs.Enabled) return;

            RequestLogService.Init(path);
            Logs.RequestHandler = new AsyncRequestHandler<RequestLog>(TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), 1).Start();
            Logs.RequestHandler.Subscribe(RequestLogService.WriteLog);

            //StatusDir500 = Path.Combine(path, StatusDir500);
            //StatusDir500Exists = Directory.Exists(StatusDir500);

            ExceptionLogService.Init(path);
            Logs.ExceptionHandler = new AsyncExceptionHandler<ExceptionLog>(TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), 1).Start();
            Logs.ExceptionHandler.Subscribe(ExceptionLogService.WriteLog);

            // Global Exception Handler for Status 404 ～ 500 Internal Server Error
            services.AddExceptionHandler(ExceptionHandler);
        }

        /// <summary>
        /// Global Exception Handler for Status 404 ~ 500 Internal Server Error
        /// </summary>
        /// <param name="options"></param>
        public static void ExceptionHandler(ExceptionHandlerOptions options)
        {
            // Passing by 404
            options.AllowStatusCode404Response = true;
            // The path to the exception handling endpoint for MVC
            //options.ExceptionHandlingPath = new PathString("/error");
            // Handle the exception
            options.ExceptionHandler = context =>
            {
                //var pathError = context.Features.Get<IExceptionHandlerPathFeature>().Error; // No Passing by 404
                var e = context.Features.Get<IExceptionHandlerFeature>().Error;
                if (e is WebException)
                {
                    context.Abort();
                    return Task.CompletedTask;
                }

                var text = string.Empty;
                if (e is TaskCanceledException || e is OperationCanceledException)
                {
                    context.Response.StatusCode = StatusCodes.Status204NoContent;
                    return context.Response.WriteAsync(text);
                }

                // Check if it is an API request
                const int status = StatusCodes.Status500InternalServerError;
                if (!context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = status;
                    return context.Response.WriteAsync(e.Message);
                }

                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = status;

                string trace = context.TraceIdentifier, url = context.Request.GetDisplayUrl();
                string detail = e.ToString(), details = detail;
                string[] s = detail.Split(Environment.NewLine);
                if (s.Length > 3) detail = string.Join(" ↓", s[0], s[1], s[2]);
                var error = new { title = e.Message, detail, trace, status };

                // Record logs, if exists web logs/500 directory
                //if (!StatusDir500Exists) Serilog.Log.Logger.Error(e, url);
                var contents = new StringBuilder(url);
                contents.Append(Environment.NewLine);
                contents.Append(Environment.NewLine);

                // Gets trace request contents
                if (trace != null && context.Items.TryGetValue(trace, out object value) && value != null)
                {
                    if (value is string body)
                    {
                        contents.Append(" body => ");
                        contents.Append(string.IsNullOrWhiteSpace(body) ? "null" : body);
                        contents.Append(Environment.NewLine);
                    }
                    else if (value is IDictionary<string, object> input)
                    {
                        foreach (var key in input.Keys)
                        {
                            contents.Append($" {key} => ");
                            contents.Append(input[key]?.ToJson() ?? "null");
                            contents.Append(Environment.NewLine);
                        }
                    }
                    if (context.Items.TryGetValue(trace + "sql", out object sqlValue))
                    {
                        contents.Append(Environment.NewLine);
                        contents.Append($" sql => {sqlValue}");
                    }
                }

                // Write origin error logs
                contents.Append(Environment.NewLine);
                contents.Append(details);

                // Asynchronous record log file
                Logs.ExceptionHandler.Publish(new ExceptionLog
                {
                    Path = context.Request.Path.Value,
                    Trace = trace,
                    Message = e.Message,
                    Content = contents.ToString(),
                    Time = DateTime.Now,
                });

                text = Newtonsoft.Json.JsonConvert.SerializeObject(error);

                return context.Response.WriteAsync(text);
            };
        }


        /// <summary>
        /// Global Monitor System
        /// Sentry: Exception Monitoring Platform
        /// https://sentry.io/signup
        /// https://github.com/docker-library/docs/tree/master/sentry
        /// https://github.com/getsentry/onpremise > ./install.sh (docker)
        /// </summary>
        public static IWebHostBuilder UseSentryMonitor(this IWebHostBuilder builder)
        {
            //var section = config.GetSection(SentrySettings.AppSettings);
            //if (!section.Exists()) return services;

            //// Register IOptions<SentrySettings> from appsettings.json
            //services.Configure<SentrySettings>(section);
            //config.Bind(SentrySettings.AppSettings, SentrySettings.Instance);

            //builder.UseSentry((context, options) =>
            //{
            //    options.Environment = context.HostingEnvironment.EnvironmentName;
            //    options.Dsn = context.Configuration.GetSection("Sentry:Dsn").Value;
            //    options.Debug = bool.Parse(context.Configuration.GetSection("Sentry:Debug").Value);
            //    options.SendDefaultPii = bool.Parse(context.Configuration.GetSection("Sentry:SendDefaultPii").Value);
            //    options.AttachStacktrace = bool.Parse(context.Configuration.GetSection("Sentry:AttachStacktrace").Value);
            //    options.MaxRequestBodySize = Enum.Parse<Sentry.Extensibility.RequestSize>(context.Configuration.GetSection("Sentry:MaxRequestBodySize").Value);
            //    options.MinimumBreadcrumbLevel = Enum.Parse<Microsoft.Extensions.Logging.LogLevel>(context.Configuration.GetSection("Sentry:MinimumBreadcrumbLevel").Value);
            //    options.MinimumEventLevel = Enum.Parse<Microsoft.Extensions.Logging.LogLevel>(context.Configuration.GetSection("Sentry:MinimumEventLevel").Value);
            //    options.DiagnosticLevel = Enum.Parse<Sentry.SentryLevel>(context.Configuration.GetSection("Sentry:DiagnosticLevel").Value);
            //    options.BeforeSend = (e) => e.Exception is TaskCanceledException || e.Exception is OperationCanceledException ? null : e;
            //});

            return builder;
        }
    }
}
