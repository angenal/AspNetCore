using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using WebInterface.Settings;

namespace WebFramework.Filters
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
            if (context.Controller is ApiController controller)
            {
                if (controller.user == null && context.HttpContext.User != null && context.HttpContext.User.HasClaim(c => c.Type == JwtSettings.NameClaimType))
                {
                    controller.user = context.HttpContext.User.Session();
                }
            }
            await next();
        }
    }
}
