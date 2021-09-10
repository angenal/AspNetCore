using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebFramework.Services;

namespace WebFramework.Filters
{
    /// <summary></summary>
    public class AsyncTraceMonitorFilter : IAsyncActionFilter
    {
        /// <summary></summary>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var trace = context.HttpContext.TraceIdentifier;
            if (trace != null)
            {
                // Web logs record cache enabled
                if (Logs.Enabled && context.ActionArguments.Count > 0)
                {
                    context.HttpContext.Items.TryAdd(trace, context.ActionArguments);
                }
                // Web logs record post request
                else if (context.HttpContext.Request.Method.Equals(HttpMethods.Post) && context.ActionDescriptor.ActionConstraints.Any(t => t is HttpMethodActionConstraint c && c.HttpMethods.Contains(HttpMethods.Post)))
                {
                    // Enable seeking
                    context.HttpContext.Request.EnableBuffering();
                    // Read the stream as text
                    if (context.HttpContext.Request.Body != null && context.HttpContext.Request.Body.CanRead)
                    {
                        var body = await new StreamReader(context.HttpContext.Request.Body).ReadToEndAsync();
                        context.HttpContext.Items.TryAdd(trace, body);
                    }
                    // Set the position of the stream to 0 to enable rereading
                    context.HttpContext.Request.Body.Position = 0; //SeekOrigin.Begin
                }
            }
            await next();
        }
    }
}
