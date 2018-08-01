using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebCore.Annotations;
using WebCore.Data.DTO;
using WebCore.Utils;
using WebFramework.Extensions;

namespace WebFramework.Authentications
{
    /// <summary>
    /// api认证: /api/auth
    /// api authorized middleware
    /// </summary>
    public class ApiAuthorized
    {
        public ApiAuthorized(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        /// <summary>
        /// 把 api认证 注册到服务容器中
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiAuthorized(options =>
            {
                options.Name = Configuration.GetSection("ApiAuthorized")["Name"];
                options.SecretKey = Configuration.GetSection("ApiAuthorized")["SecretKey"];
                options.ExpiresIn = Convert.ToInt32(Configuration.GetSection("ApiAuthorized")["ExpiresIn"]);
                options.Path = Configuration.GetSection("ApiAuthorized")["Path"];
            });
        }
        /// <summary>
        /// 把 api认证 添加到 Http处理Pipeline中
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();

            //api authorized middleware
            app.UseApiAuthorized();

            //app.UseMvc();
        }
        /// <summary>
        /// 初始化 App.Infos
        /// </summary>
        /// <param name="action"></param>
        public void ConfigAppAsync(Func<List<IAppInfo>> action, bool _async = false)
        {
            if (_async)
            {
                action.BeginInvoke(iar => App.Infos = action.EndInvoke(iar), null);
            }
            else
            {
                App.Infos = action.Invoke();
            }
        }
        /// <summary>
        /// 初始化 User.Infos
        /// </summary>
        /// <param name="action"></param>
        public void ConfigUserAsync(Func<List<IUserInfo>> action, bool _async = false)
        {
            if (_async)
            {
                action.BeginInvoke(iar => User.Infos = action.EndInvoke(iar), null);
            }
            else
            {
                User.Infos = action.Invoke();
            }
        }

        #region internal var

        internal IConfiguration Configuration { get; }
        #endregion
    }
    /// <summary>
    /// 依赖注入 services.AddApiAuthorized 服务
    /// </summary>
    public static class ApiAuthorizedServicesExtensions
    {
        /// <summary>
        /// Add response compression services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> for adding services.</param>
        /// <returns></returns>
        public static IServiceCollection AddApiAuthorized(this IServiceCollection services)
        {
            return services;
        }

        /// <summary>
        /// Add response compression services and configure the related options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> for adding services.</param>
        /// <param name="configureOptions">A delegate to configure the <see cref="ResponseCompressionOptions"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddApiAuthorized(this IServiceCollection services, Action<ApiAuthorizedOptions> configureOptions)
        {
            services.Configure(configureOptions);
            return services;
        }
    }
    /// <summary>
    /// 添加中间件 app.UseApiAuthorized 到 Pipeline
    /// </summary>
    public static class ApiAuthorizedMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiAuthorized([NotNull]this IApplicationBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            return builder.UseMiddleware<ApiAuthorizedMiddleware>();
        }

        public static IApplicationBuilder UseApiAuthorized(this IApplicationBuilder builder, ApiAuthorizedOptions options)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NotNull(options, nameof(options));

            return builder.UseMiddleware<ApiAuthorizedMiddleware>(Options.Create(options));
        }
    }
    public class ApiAuthorizedOptions
    {
        /// <summary>
        /// 凭证名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 凭证密钥Key
        /// </summary>
        public string SecretKey { get; set; }
        /// <summary>
        /// 凭证有效时间，单位：秒
        /// </summary>
        public int ExpiresIn { get; set; }
        /// <summary>
        /// 需要认证的路径 - 正则表达式
        /// </summary>
        public string Path { get; set; }
    }
    public class ApiAuthorizedMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ApiAuthorizedOptions options;

        public ApiAuthorizedMiddleware(RequestDelegate next, IOptions<ApiAuthorizedOptions> options)
        {
            this.next = next;
            this.options = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Response.HasStarted)
            {
                var path = context.Request.Path;
                if (!string.IsNullOrWhiteSpace(options.Path) && path.HasValue
                    && !path.Value.EndsWith("favicon.ico")
                    && new Regex(options.Path, RegexOptions.IgnoreCase).IsMatch(path.Value))
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
                else
                {
                    await next.Invoke(context); // skip auth
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
            var response = Results.Res(401, "not authorized");
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
            var response = Results.Res(408, "timeout");
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
            bool exists = App.Infos.Exists(x => x.appid == appid && x.secret == secret);
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
            bool exists = User.Infos.Exists(x => x.userid == userid && x.secret == secret);
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
            double now_timestamp = Req.Timestamp();
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
                    var requestInfo = new ReqAppInfo
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
                    var requestInfo = new ReqUserInfo
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
                    var requestInfo = new ReqAppInfo
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
                    var requestInfo = new ReqUserInfo
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
        private async Task CheckApp(HttpContext context, ReqAppInfo req)
        {
            string computeSinature = Req.Sinature_HMACMD5(req.appid, req.timestamp, req.nonce, options.SecretKey);
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
        private async Task CheckUser(HttpContext context, ReqUserInfo req)
        {
            string computeSinature = Req.Sinature_HMACMD5(req.userid, req.timestamp, req.nonce, options.SecretKey);
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
