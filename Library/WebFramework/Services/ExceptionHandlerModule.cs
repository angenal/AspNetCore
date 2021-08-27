using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebFramework.Services
{
    /// <summary>
    /// Configure Global monitoring and exception handler module
    /// </summary>
    public static class ExceptionHandlerModule
    {
        /// <summary>
        /// Web logs files directory
        /// </summary>
        public const string FilesDirectory = "files";

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

                string detail = e.ToString(), url = context.Request.GetDisplayUrl(), details = detail;
                var s = detail.Split(Environment.NewLine);
                if (s.Length > 3) detail = string.Join(" → ", s[0], s[1], s[2]);

                var error = new
                {
                    title = e.Message,
                    detail,
                    trace = context.TraceIdentifier,
                    status,
                };

                // Record logs, if exists Web logs files directory
                var path = Path.Combine(context.Features.Get<IWebHostEnvironment>().WebRootPath, FilesDirectory);
                if (Directory.Exists(path))
                {
                    path = Path.Combine(path, status.ToString());
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    details = $"{url}{Environment.NewLine}{Environment.NewLine}{details}";
                    File.WriteAllTextAsync(Path.Combine(path, $"{error.trace}.txt"), details).Start();
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
