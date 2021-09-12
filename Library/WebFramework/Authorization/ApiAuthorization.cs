using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebCore;
using WebCore.Annotations;

namespace WebFramework.Authorization
{
    /// <summary>
    /// 依赖注入 services.AddApiAuthorization 服务
    /// </summary>
    public static class ApiAuthorizationServicesExtensions
    {
        /// <summary>
        /// 添加 Api认证授权 服务
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> for adding services.</param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddApiAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            ApiAuthorization.Authorization = new ApiAuthorization(configuration);
            ApiAuthorization.Authorization.ConfigureServices(services);
            return services;
        }
        /// <summary>
        /// 添加 Api认证授权 服务
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> for adding services.</param>
        /// <param name="configuration"></param>
        /// <param name="configureOptions">A delegate to configure the <see cref="ResponseCompressionOptions"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddApiAuthorization(this IServiceCollection services, IConfiguration configuration, Action<ApiAuthorizationOptions> configureOptions)
        {
            ApiAuthorization.Authorization = new ApiAuthorization(configuration);
            services.Configure(configureOptions);
            return services;
        }
    }
    /// <summary>
    /// 添加中间件 app.UseApiAuthorization 到 Pipeline
    /// </summary>
    public static class ApiAuthorizationMiddlewareExtensions
    {
        /// <summary>
        /// 添加中间件 Api认证授权
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseApiAuthorization([NotNull] this IApplicationBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            return builder.UseMiddleware<ApiAuthorizationMiddleware>();
        }
        /// <summary>
        /// 添加中间件 Api认证授权
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseApiAuthorization(this IApplicationBuilder builder, ApiAuthorizationOptions options)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(options, nameof(options));

            return builder.UseMiddleware<ApiAuthorizationMiddleware>(Options.Create(options));
        }
    }
    /// <summary>
    /// Api认证授权:/api/auth
    /// </summary>
    public class ApiAuthorization
    {
        internal static ApiAuthorization Authorization;

        /// <summary></summary>
        public ApiAuthorization(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 把 api认证 注册到服务容器中
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiAuthorization(Configuration, options =>
            {
                options.Name = Configuration.GetSection("ApiAuthorization")["Name"];
                options.Secret = Configuration.GetSection("ApiAuthorization")["Secret"];
                options.ExpiresIn = Convert.ToInt32(Configuration.GetSection("ApiAuthorization")["ExpiresIn"]);
                options.Path = Configuration.GetSection("ApiAuthorization")["Path"];
            });
        }
        /// <summary>
        /// 初始化 App.Infos
        /// </summary>
        /// <param name="action"></param>
        /// <param name="_async"></param>
        public static void ConfigAppAsync(Func<List<WebCore.Data.DTO.IAppInfo>> action, bool _async = false)
        {
            if (_async)
            {
                action.BeginInvoke(iar => WebCore.Data.DTO.App.Infos = action.EndInvoke(iar), null);
            }
            else
            {
                WebCore.Data.DTO.App.Infos = action.Invoke();
            }
        }
        /// <summary>
        /// 初始化 User.Infos
        /// </summary>
        /// <param name="action"></param>
        /// <param name="_async"></param>
        public static void ConfigUserAsync(Func<List<WebCore.Data.DTO.IUserInfo>> action, bool _async = false)
        {
            if (_async)
            {
                action.BeginInvoke(iar => WebCore.Data.DTO.User.Infos = action.EndInvoke(iar), null);
            }
            else
            {
                WebCore.Data.DTO.User.Infos = action.Invoke();
            }
        }

        /// <summary></summary>
        internal IConfiguration Configuration { get; }
    }
    /// <summary>
    /// Api认证授权:选项
    /// </summary>
    public class ApiAuthorizationOptions
    {
        /// <summary>
        /// 凭证名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 凭证密钥Key
        /// </summary>
        public string Secret { get; set; }
        /// <summary>
        /// 凭证有效时间，单位：秒
        /// </summary>
        public int ExpiresIn { get; set; }
        /// <summary>
        /// 需要认证的路径 - 正则表达式
        /// </summary>
        public string Path { get; set; }
    }
    /// <summary>
    /// Api认证授权:中间件
    /// </summary>
    public class ApiAuthorizationMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ApiAuthorizationOptions options;

        /// <summary></summary>
        public ApiAuthorizationMiddleware(RequestDelegate next, IOptions<ApiAuthorizationOptions> options)
        {
            this.next = next;
            this.options = options.Value;
        }

        /// <summary></summary>
        public async Task Invoke(HttpContext context)
        {
            if (!context.Response.HasStarted)
            {
                var path = context.Request.Path;
                if (options.Path == null || !path.StartsWithSegments("/api") || !new Regex(options.Path, RegexOptions.IgnoreCase).IsMatch(path.Value))
                {
                    await next.Invoke(context); // skip auth
                }
                else
                {
                    switch (context.Request.Method.ToUpper())
                    {
                        case "PATCH":
                        case "POST":
                        case "PUT":
                            await PostInvoke(context); // post auth
                            break;
                        default:
                            await GetInvoke(context); // get auth
                            break;
                    }
                    if (!context.Response.HasStarted)
                    {
                        await next.Invoke(context); // skip auth
                    }
                }
            }
            else
            {
                await next.Invoke(context); // skip auth
            }
        }

        #region private method
        /// <summary>
        /// 401 not authorized
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task ReturnNoAuthorized(HttpContext context)
        {
            context.Response.StatusCode = 401;
            var response = WebCore.Data.DTO.Results.Res(401, "not authorized");
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
        /// <summary>
        /// 408 timeout
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task ReturnTimeOut(HttpContext context)
        {
            context.Response.StatusCode = 408;
            var response = WebCore.Data.DTO.Results.Res(408, "timeout");
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }

        /// <summary>
        /// check the application - after config App.Infos
        /// </summary>
        /// <param name="context"></param>
        /// <param name="appid"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        private async Task CheckApp(HttpContext context, string appid, string secret)
        {
            bool exists = WebCore.Data.DTO.App.Infos.Exists(x => x.appid == appid && x.secret == secret);
            if (!exists) await ReturnNoAuthorized(context);
        }
        /// <summary>
        /// check the user - after config User.Infos
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userid"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        private async Task CheckUser(HttpContext context, string userid, string secret)
        {
            bool exists = WebCore.Data.DTO.User.Infos.Exists(x => x.userid == userid && x.secret == secret);
            if (!exists) await ReturnNoAuthorized(context);
        }

        /// <summary>
        /// check the expired time
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="expiresIn"></param>
        /// <returns></returns>
        private bool CheckExpiredTime(long timestamp, long expiresIn)
        {
            double now_timestamp = WebCore.Data.DTO.Req.Timestamp();
            return (now_timestamp - timestamp) > expiresIn;
        }

        /// <summary>
        /// http get invoke
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task GetInvoke(HttpContext context)
        {
            var useHeader = context.Request.Headers.ContainsKey("Authorization");
            if (useHeader)
            {
            }
            var pairs = context.Request.Query;
            if (pairs.ContainsKeys("timestamp nonce signature secret".Split(' ')))
            {
                if (pairs.ContainsKey("appid"))
                {
                    var requestInfo = new WebCore.Data.DTO.ReqAppInfo
                    {
                        timestamp = pairs["timestamp"].ToString(),
                        nonce = pairs["nonce"].ToString(),
                        sinature = pairs["signature"].ToString(),
                        appid = pairs["appid"].ToString(),
                        secret = pairs["secret"].ToString(),
                    };
                    await CheckApp(context, requestInfo);
                }
                else if (pairs.ContainsKey("userid"))
                {
                    var requestInfo = new WebCore.Data.DTO.ReqUserInfo
                    {
                        timestamp = pairs["timestamp"].ToString(),
                        nonce = pairs["nonce"].ToString(),
                        sinature = pairs["signature"].ToString(),
                        userid = pairs["userid"].ToString(),
                        secret = pairs["secret"].ToString(),
                    };
                    await CheckUser(context, requestInfo);
                }
                else
                {
                    await ReturnNoAuthorized(context);
                }
            }
            else
            {
                await ReturnNoAuthorized(context);
            }
        }

        /// <summary>
        /// http post invoke
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task PostInvoke(HttpContext context)
        {
            var pairs = context.Request.Form;
            if (context.Request.HasFormContentType
                && pairs.ContainsKeys("timestamp nonce signature secret".Split(' ')))
            {
                if (pairs.ContainsKey("appid"))
                {
                    var requestInfo = new WebCore.Data.DTO.ReqAppInfo
                    {
                        timestamp = pairs["timestamp"].ToString(),
                        nonce = pairs["nonce"].ToString(),
                        sinature = pairs["signature"].ToString(),
                        appid = pairs["appid"].ToString(),
                        secret = pairs["secret"].ToString(),
                    };
                    await CheckApp(context, requestInfo);
                }
                else if (pairs.ContainsKey("userid"))
                {
                    var requestInfo = new WebCore.Data.DTO.ReqUserInfo
                    {
                        timestamp = pairs["timestamp"].ToString(),
                        nonce = pairs["nonce"].ToString(),
                        sinature = pairs["signature"].ToString(),
                        userid = pairs["userid"].ToString(),
                        secret = pairs["secret"].ToString(),
                    };
                    await CheckUser(context, requestInfo);
                }
                else
                {
                    await ReturnNoAuthorized(context);
                }
            }
            else
            {
                await ReturnNoAuthorized(context);
            }
        }

        /// <summary>
        /// the main check method - Sinature_HMACMD5
        /// </summary>
        /// <param name="context"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        private async Task CheckApp(HttpContext context, WebCore.Data.DTO.ReqAppInfo req)
        {
            string computeSinature = WebCore.Data.DTO.Req.Sinature_HMACMD5(req.appid, req.timestamp, req.nonce, options.Secret);
            if (computeSinature.Equals(req.sinature) && long.TryParse(req.timestamp, out long tmpTimestamp))
            {
                if (CheckExpiredTime(tmpTimestamp, options.ExpiresIn))
                {
                    await ReturnTimeOut(context);
                }
                else
                {
                    await CheckApp(context, req.appid, req.secret);
                }
            }
            else
            {
                await ReturnNoAuthorized(context);
            }
        }
        /// <summary>
        /// the main check method - Sinature_HMACMD5
        /// </summary>
        /// <param name="context"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        private async Task CheckUser(HttpContext context, WebCore.Data.DTO.ReqUserInfo req)
        {
            string computeSinature = WebCore.Data.DTO.Req.Sinature_HMACMD5(req.userid, req.timestamp, req.nonce, options.Secret);
            if (computeSinature.Equals(req.sinature) && long.TryParse(req.timestamp, out long tmpTimestamp))
            {
                if (CheckExpiredTime(tmpTimestamp, options.ExpiresIn))
                {
                    await ReturnTimeOut(context);
                }
                else
                {
                    await CheckUser(context, req.userid, req.secret);
                }
            }
            else
            {
                await ReturnNoAuthorized(context);
            }
        }

        #endregion
    }
}
