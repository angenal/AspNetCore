using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using WebSwaggerDemo.NET5.Common;

namespace WebSwaggerDemo.NET5.Filters
{
    /// <summary>
    /// 用户会话处理 by Jwt Token
    /// </summary>
    public class AsyncSessionFilter : IAsyncActionFilter
    {
        /// <summary>
        /// 当前用户会话
        /// </summary>
        public Session user;

        /// <summary></summary>
        public AsyncSessionFilter(Session session) => user = session;

        /// <summary></summary>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.Controller is ControllerBase controller)
            {
                if (controller.HttpContext.Items["Session"] == null && context.HttpContext.User != null && context.HttpContext.User.HasClaim(c => c.Type == JwtSettings.NameClaimType))
                {
                    controller.HttpContext.Items["Session"] = context.HttpContext.User.Session();
                }
            }
            await next();
        }
    }
}
