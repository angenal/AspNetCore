using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCore;
using WebFramework.Services;

namespace WebFramework.Filters
{
    /// <summary></summary>
    public class AsyncTraceMonitorFilter : IActionFilter, IOrderedFilter
    {
        /// <summary>
        /// A filter that specifies the relative order it should run.
        /// </summary>
        public int Order { get; } = int.MaxValue - 11;

        /// <summary></summary>
        public bool Enabled { get; set; }
        /// <summary></summary>
        public string Trace { get; set; }

        /// <summary></summary>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Gets a unique identifier to represent this request in trace logs.
            Trace = context.HttpContext.TraceIdentifier;
            // Trace limit request byte size, less than 1MB
            Enabled = Trace != null;
            if (Enabled && (Logs.Enabled || Logs.Manage.Trace))
            {
                // Record input arguments
                if (context.ActionArguments.Count > 0)
                {
                    context.HttpContext.Items.TryAdd(Trace, context.ActionArguments);
                }
                // Record post request body
                else if (context.HttpContext.Request.Method.Equals(HttpMethods.Post)
                    && context.ActionDescriptor.ActionConstraints.Any(t => t is HttpMethodActionConstraint c && c.HttpMethods.Contains(HttpMethods.Post))
                    && context.HttpContext.Request.Body?.Length < Logs.Manage.Limit)
                {
                    switch (context.HttpContext.Request.ContentType.ToLower())
                    {
                        case "application/json":
                        case "application/x-www-form-urlencoded":
                            OnPostRequestAsync(context, Trace).Wait();
                            break;
                        case "multipart/form-data":
                            var hasUpload = context.HttpContext.Request.Headers.TryGetValue("Content-Disposition", out var contentDisposition) && contentDisposition.Any(c => c.Contains("filename", StringComparison.OrdinalIgnoreCase));
                            if (!hasUpload) OnPostRequestAsync(context, Trace).Wait();
                            break;
                    }
                }
            }
        }
        /// <summary></summary>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (Enabled && Logs.Manage.Trace && context.Exception == null) TraceRecord(context.HttpContext, Trace);
        }
        /// <summary>Record post request body</summary>
        private static async Task OnPostRequestAsync(ActionExecutingContext context, string trace)
        {
            // Enable seeking
            context.HttpContext.Request.EnableBuffering();
            // Read the stream as text
            if (context.HttpContext.Request.Body?.Length > 0 && context.HttpContext.Request.Body.CanRead)
            {
                var body = await new StreamReader(context.HttpContext.Request.Body).ReadToEndAsync();
                context.HttpContext.Items.TryAdd(trace, body);
            }
            // Set the position of the stream to 0 to enable rereading
            context.HttpContext.Request.Body.Position = 0; // SeekOrigin.Begin
        }
        /// <summary>Trace request</summary>
        private static void TraceRecord(HttpContext context, string trace)
        {
            string url = context.Request.GetDisplayUrl();
            var contents = new StringBuilder(url);
            if (context.Items.TryGetValue(trace, out object value) && value != null)
            {
                contents.Append(Environment.NewLine);
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
                contents.Append(Environment.NewLine);
            }
            var res = new StringBuilder();
            res.AppendLine($"ContentType: {(context.Response.ContentType ?? "application/json")}");
            res.AppendLine($"StatusCode: {context.Response.StatusCode}");
            //res.Append(Environment.NewLine);
            //if (context.Response.Body?.Length > 0 && context.Response.ContentType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
            //    res.Append(context.Response.Body);
            // Asynchronous record log file
            Logs.RequestHandler.Publish(new RequestLog
            {
                Path = context.Request.Path.Value,
                Trace = trace,
                Request = contents.ToString(),
                Response = res.ToString(),
                Time = DateTime.Now,
            });
        }
    }
}
