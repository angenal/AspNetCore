using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebCore;

namespace WebFramework.Services
{
    /// <summary>
    /// Configure Global monitoring and exception handler module
    /// </summary>
    public static class ExceptionHandlerModule
    {
        /// <summary>
        /// Web logs root directory
        /// </summary>
        public const string LogsRootDir = "logs";
        static string StatusDir500 = StatusCodes.Status500InternalServerError.ToString();
        /// <summary>
        /// Web logs record status 500
        /// </summary>
        public static bool StatusDir500Exists = false;
        /// <summary>
        /// Web logs record cache enabled
        /// </summary>
        public static bool CacheEnabled = false;

        /// <summary>
        /// Init Exception Module
        /// </summary>
        public static void Init(IConfiguration config, IWebHostEnvironment env)
        {
            var path = Path.Combine(env.WebRootPath, LogsRootDir);
            if (!Directory.Exists(path)) return;
            StatusDir500 = Path.Combine(path, StatusDir500);
            StatusDir500Exists = Directory.Exists(StatusDir500);
            if (StatusDir500Exists) CacheEnabled = true;
        }

        /// <summary>
        /// Global Error Handler for Status 400 BadRequest with Invalid ModelState
        /// </summary>
        public static void ApiBehavior(ApiBehaviorOptions options)
        {
            //options.ClientErrorMapping[StatusCodes.Status404NotFound].Link = "https://*.com/404";
            //options.SuppressConsumesConstraintForFormFileParameters = true;
            //options.SuppressInferBindingSourcesForParameters = true;
            //options.SuppressModelStateInvalidFilter = true; // 关闭系统自带模型验证(使用第三方库代理)
            //options.SuppressMapClientErrors = true;
            options.InvalidModelStateResponseFactory = context =>
            {
                var s = context.ModelState.Values;
                if (context.ModelState.IsValid || !s.Any() || !s.First().Errors.Any())
                    return new OkObjectResult(context.ModelState);
                var result = new BadRequestObjectResult(new
                {
                    status = 400,
                    title = s.First().Errors.First().ErrorMessage.Replace("＆", " "),
                    errors = string.Join("；", s.Select(v => string.Join("；", v.Errors.Select(e => e.ErrorMessage)))).Replace("＆", " ")
                });
                result.ContentTypes.Add(System.Net.Mime.MediaTypeNames.Application.Json);
                return result;
            };
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

                var url = context.Request.GetDisplayUrl();
                string detail = e.ToString(), details = detail;
                string[] s = detail.Split(Environment.NewLine);
                if (s.Length > 3) detail = string.Join(" ↓", s[0], s[1], s[2]);
                var error = new { title = e.Message, detail, trace = context.TraceIdentifier, status };

                // Record logs, if exists web logs directory
                if (StatusDir500Exists)
                {
                    var contents = new StringBuilder();
                    //contents.Append(Environment.NewLine);
                    contents.Append(url);
                    contents.Append(Environment.NewLine);
                    contents.Append(Environment.NewLine);

                    //  Enable seeking
                    //context.Request.EnableBuffering();
                    //  Read the stream as text
                    //if (context.Request.Body != null && context.Request.Body.CanRead)
                    //{
                    //    var body = new StreamReader(context.Request.Body).ReadToEndAsync().GetAwaiter().GetResult();
                    //    if (!string.IsNullOrWhiteSpace(body))
                    //    {
                    //        contents.Append(body);
                    //        contents.Append(Environment.NewLine);
                    //        contents.Append(Environment.NewLine);
                    //    }
                    //}
                    //  Set the position of the stream to 0 to enable rereading
                    //context.Request.Body.Position = 0;

                    if (error.trace != null && context.Items.TryGetValue(error.trace, out object value) && value is IDictionary<string, object> input)
                    {
                        foreach (var key in input.Keys)
                        {
                            contents.Append($" {key} = ");
                            contents.Append(input[key]?.ToJson() ?? "null");
                            contents.Append(Environment.NewLine);
                        }
                    }

                    // Write origin error
                    contents.Append(Environment.NewLine);
                    contents.Append(details);
                    contents.Append(Environment.NewLine);

                    var path = Path.Combine(StatusDir500, $"{error.trace}.txt");
                    if (File.Exists(path)) File.AppendAllText(path, contents.ToString());
                    else File.WriteAllText(path, contents.ToString());
                }
                else
                {
                    Serilog.Log.Logger.Error(e, url);
                }

                text = Newtonsoft.Json.JsonConvert.SerializeObject(error);

                return context.Response.WriteAsync(text);
            };
        }

        /// <summary>
        /// Global Exception Filters for MVC
        /// </summary>
        /// <param name="options"></param>
        public static void ApiExceptionsFilters(MvcOptions options)
        {
            //options.Filters.Add<HttpResponseExceptionFilter>();
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
