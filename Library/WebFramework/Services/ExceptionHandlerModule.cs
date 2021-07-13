using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebFramework.Services
{
    /* appsettings.json
      "Sentry": {
        "Dsn": "https://0357ef2d9cfd4e77a8fd05599bc385c8@o301489.ingest.sentry.io/5426676",
        "Debug": false,
        "SendDefaultPii": true,
        "AttachStacktrace": true,
        "MaxRequestBodySize": "Small",
        "MinimumBreadcrumbLevel": "Debug",
        "MinimumEventLevel": "Warning",
        "DiagnosticLevel": "Error"
      }
    */

    /// <summary>
    /// Global monitoring and exception handler module
    /// </summary>
    public static class ExceptionHandlerModule
    {
        /// <summary>
        /// Configure Monitoring Module
        /// Sentry: Exception Monitoring Platform
        /// https://sentry.io/signup
        /// https://github.com/docker-library/docs/tree/master/sentry
        /// https://github.com/getsentry/onpremise > ./install.sh (docker)
        /// </summary>
        public static IWebHostBuilder UseSentryMonitor(this IWebHostBuilder builder)
        {
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

        /// <summary>
        /// Configure BadRequest Error Handler with Invalid ModelState Response
        /// </summary>
        public static void ApiBehaviorOptionsAction(ApiBehaviorOptions options)
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                if (context.ModelState.IsValid) return new OkObjectResult(context.ModelState);
                var result = new BadRequestObjectResult(new
                {
                    status = 400,
                    title = Convert.ToString(context.ModelState.Values.First().Errors.First().ErrorMessage).Replace("＆", " "),
                    errors = Convert.ToString(string.Join("；", context.ModelState.Values.Select(v => string.Join("；", v.Errors.Select(e => e.ErrorMessage))))).Replace("＆", " ")
                });
                result.ContentTypes.Add(System.Net.Mime.MediaTypeNames.Application.Json);
                return result;
            };
        }

        /// <summary>
        /// Configure Global Exception Handler
        /// </summary>
        /// <param name="options"></param>
        public static void ExceptionHandlerOptionsAction(ExceptionHandlerOptions options)
        {
            // Passing by 404
            options.AllowStatusCode404Response = true;
            // Exception Handler
            options.ExceptionHandler = context =>
            {
                var e = context.Features.Get<IExceptionHandlerFeature>().Error;
                if (e is IOException || e is WebException)
                {
                    return Task.CompletedTask;
                }

                var text = string.Empty;
                if (e is TaskCanceledException || e is OperationCanceledException)
                {
                    context.Response.StatusCode = StatusCodes.Status204NoContent;
                    return context.Response.WriteAsync(text);
                }

                //context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                text = Newtonsoft.Json.JsonConvert.SerializeObject(new { status = 500, title = e.Message });

                return context.Response.WriteAsync(text);
            };
        }
    }
}
