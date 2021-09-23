using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;
using WebCore;
using WebFramework.Authentication.WeixinMiniProgram;

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
            var section = config.GetSection(WeixinLoginDefaults.AppSettings);
            if (!section.Exists()) return services;

            builder.AddWeixinMiniProgram(options =>
            {
                options.AppId = section.GetValue<string>("appid");
                options.Secret = section.GetValue<string>("secret");
                options.CustomerLoginState += context =>
                {
                    context.HttpContext.Response.Redirect($"{WeixinAuthenticationService.GetWeixinMiniProgramTokenUrl}?key={context.SessionInfoKey}");
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
        /// 自定义逻辑：处理微信OpenId与该系统的关系
        /// </summary>
        public static Func<WeixinLoginStateContext, Task<Session>> GetSessionFromWeixinMiniProgram { get; set; } = context => Task.FromResult(new Session());

        /// <summary>
        /// Login weixin mini program
        /// </summary>
        public const string GetWeixinMiniProgramTokenUrl = "/api/WxOpen/CreateToken";

        /// <summary>
        /// Login weixin mini program, return token.
        /// </summary>
        public static async Task GetWeixinMiniProgramTokenHandler(HttpContext context)
        {
            string key = context.Request.Query["key"].ToString();
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"参数 key 不能为空");

            var service = context.RequestServices.GetService<IWeixinLoginStateInfoStore>();
            var session = service.GetSessionInfo(key).ConfigureAwait(false).GetAwaiter().GetResult();
            //session.OpenId;
            var text = session.ToJson();
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(text);
        }

        /// <summary>
        /// Login weixin mini program, return token.
        /// </summary>
        public async Task CreateToken(WeixinLoginStateContext context)
        {
            if (context.ErrCode != null && !context.ErrCode.Equals("0"))
            {
                var error = new ErrorJsonResultObject { Status = (int)HttpStatusCode.BadRequest, Title = context.ErrMsg };
                context.Response.StatusCode = error.Status;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(error.ToJson());
                return;
            }
            var o = await GetSessionFromWeixinMiniProgram(context) ?? new Session();
            var session = JObject.FromObject(o);
            if (!string.IsNullOrEmpty(o.Id)) session["token"] = context.HttpContext.RequestServices.GetService<IJwtGenerator>()?.Generate(o.Claims());
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(session.ToJson());
        }

    }
}
