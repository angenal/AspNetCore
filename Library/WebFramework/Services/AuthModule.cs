using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using WebCore;
using WebInterface.Settings;

namespace WebFramework.Services
{
    /// <summary>
    /// Api Authentication + Authorization Module
    /// </summary>
    public static class AuthModule
    {
        /// <summary>
        /// Register Authentication + Authorization services
        /// </summary>
        public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true; // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Custom Api Authorization using WebFramework.Authorization
            //services.AddApiAuthorization(config);

            // Microsoft.AspNetCore.Identity system for the specified User and Role types
            //services.AddIdentityLiteDB(config);

            // Authentication with JWT
            services.AddJwtAuthentication(config);

            // Authentication with OAuth
            var section = config.GetSection("OAuth");
            if (section.Exists())
            {
                var oAuth = services.AddAuthentication();
                var qqSection = section.GetSection("QQ");
                if (qqSection.Exists())
                {
                    // 回调地址: /signin-qq
                    string appid = qqSection.GetValue<string>("ClientId"), secret = qqSection.GetValue<string>("ClientSecret");
                    if (!string.IsNullOrEmpty(appid) && !string.IsNullOrEmpty(secret)) oAuth.AddQQAuthentication(t =>
                    {
                        t.ClientId = appid;
                        t.ClientSecret = secret;
                    });
                }
                var wxSection = section.GetSection("Weixin");
                if (wxSection.Exists())
                {
                    // 回调地址: /signin-weixin
                    string appid = wxSection.GetValue<string>("ClientId"), secret = wxSection.GetValue<string>("ClientSecret");
                    if (!string.IsNullOrEmpty(appid) && !string.IsNullOrEmpty(secret)) oAuth.AddWeixinAuthentication(t =>
                    {
                        t.ClientId = appid;
                        t.ClientSecret = secret;
                    });
                }
                var wxmSection = section.GetSection("WeixinMiniProgram");
                if (wxmSection.Exists())
                {
                    // 回调地址: /signin-wxopen?code=xxx
                    // 登录凭证校验。通过 wx.login 接口获得临时登录凭证 code 后传到开发者服务器调用此接口完成登录流程。
                    // https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/login/auth.code2Session.html
                    string appid = wxmSection.GetValue<string>("ClientId"), secret = wxmSection.GetValue<string>("ClientSecret");
                    if (!string.IsNullOrEmpty(appid) && !string.IsNullOrEmpty(secret)) oAuth.AddWeixinMiniProgramAuthentication(options =>
                    {
                        options.AppId = appid;
                        options.Secret = secret;
                        //options.CallbackPath = new PathString(Authentication.WeChat.WxOpen.WxOpenLoginDefaults.CallbackPath); // 修改回调地址
                        // 1.微信服务端验证完成后触发,注册该方法获取用户信息
                        //options.Events.OnWxOpenServerCompleted = context =>
                        //{
                        //    // 微信服务端返回异常信息时
                        //    if (context.ErrCode != null && !context.ErrCode.Equals("0"))
                        //    {
                        //        var error = new { errcode = context.ErrCode, errmsg = context.ErrMsg };
                        //        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        //        context.Response.ContentType = "application/json";
                        //        return context.Response.WriteAsync(error.ToJson());
                        //    }
                        //    // 自定义逻辑：处理微信OpenId与该系统的关系. 比如保存数据库
                        //    var session = new { openid = context.OpenId, unionid = context.UnionId, errcode = context.ErrCode, errmsg = context.ErrMsg };
                        //    context.Response.ContentType = "application/json";
                        //    return context.Response.WriteAsync(session.ToJson());
                        //};
                        // 2.根据微信服务器返回的会话密匙执行登录成功的操作, 比如颁发JWT, 重定向Action等
                        options.CustomerLoginState += context =>
                        {
                            //var session = new { openid = context.OpenId, unionid = context.UnionId, sessionKey = context.SessionKey };

                            // 颁发JWT
                            //var o = new Session();
                            //var session = JObject.FromObject(o);
                            //if (!string.IsNullOrEmpty(o.Id)) session["token"] = new JwtGenerator().Generate(o.Claims());

                            // 重定向 /signin-wxopen 至 /WxOpen/CreateToken (参数为缓存key) 获取缓存OpenId后创建登录凭证
                            context.HttpContext.Response.Redirect($"/WxOpen/CreateToken?cacheKey={context.SessionInfoKey.UrlEncode()}&sessionKey={context.SessionKey.UrlEncode()}&openid={context.OpenId.UrlEncode()}&unionid={context.UnionId.UrlEncode()}");
                            return Task.CompletedTask;
                        };
                    });
                }
            }


            // Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("test", policy => policy.RequireClaim("name", "测试"));
                options.AddPolicy("Upload", policy => policy.RequireAuthenticatedUser());
                options.AddPolicy("User", policy => policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == JwtSettings.RoleClaimType && c.Value.StartsWith("User")) ||
                    context.User.HasClaim(c => c.Type == "name" && c.Value.StartsWith("User"))));
            });

            return services;
        }
    }
}
