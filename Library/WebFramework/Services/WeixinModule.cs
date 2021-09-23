using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;
using WebCore;
using WebFramework.Authentication.WeChat.WxOpen;

namespace WebFramework.Services
{
    /// <summary>
    /// Weixin Module
    /// </summary>
    public static class WeixinModule
    {
        /// <summary>
        /// Register Weixin Services
        /// </summary>
        public static IServiceCollection AddWeixin(this IServiceCollection services, AuthenticationBuilder builder, IConfiguration config)
        {
            var section = config.GetSection(WxOpenLoginDefaults.AppSettings);
            if (!section.Exists()) return services;

            string appid = section.GetValue<string>("appid"), secret = section.GetValue<string>("secret");
            if (string.IsNullOrEmpty(appid) || string.IsNullOrEmpty(secret)) return services;

            builder.AddWxOpenMiniProgram(options =>
            {
                options.AppId = appid;
                options.Secret = secret;
                options.CustomerLoginState += context =>
                {
                    // 创建登录凭证 WxOpenController.CreateToken(string key)
                    context.HttpContext.Response.Redirect($"/WxOpen/CreateToken?key={context.SessionInfoKey}");
                    return Task.CompletedTask;
                };
            });

            return services;
        }
    }

    /// <summary>
    /// Weixin Authentication Service
    /// </summary>
    public class WeixinAuthenticationService
    {
        /// <summary>
        /// Login weixin mini program, return token.
        /// </summary>
        public async Task CreateToken(WxOpenLoginStateContext context)
        {
            if (context.ErrCode != null && !context.ErrCode.Equals("0"))
            {
                var error = new ErrorJsonResultObject { Status = (int)HttpStatusCode.BadRequest, Title = context.ErrMsg };
                context.Response.StatusCode = error.Status;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(error.ToJson());
                return;
            }
            // 自定义逻辑：处理微信OpenId与该系统的关系
            var openId = context.OpenId;
            var o = new Session();
            var session = JObject.FromObject(o);
            if (!string.IsNullOrEmpty(o.Id)) session["token"] = context.HttpContext.RequestServices.GetService<IJwtGenerator>()?.Generate(o.Claims());
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(session.ToJson());
        }

    }
}
