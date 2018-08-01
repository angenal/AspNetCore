using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using WebCore.Utils;

namespace WebFramework.Authentications
{
    /// <summary>
    /// 防止跨站点请求伪造 (XSRF/CSRF) 攻击: /api/
    /// web form asp-antiforgery=[默认true] | @Html.AntiForgeryToken()
    /// </summary>
    public class Antiforgery
    {
        public Antiforgery(IConfiguration configuration)
        {
            Configuration = configuration;
            HOST = Configuration["HOST"] ?? "api.com";
            Name = "CSRF-TOKEN";
        }

        public void ConfigureServices(IServiceCollection services, bool xhttpOrAngularJS = true)
        {
            services.AddAntiforgery(options =>
            {
                if (xhttpOrAngularJS)
                {
                    options.HeaderName = $"X-{Name}";
                }
                else
                {
                    options.Cookie.Domain = HOST;
                    options.Cookie.Name = Name;
                    options.Cookie.Path = "/";
                    options.FormFieldName = "__RequestVerificationToken";
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.SuppressXFrameOptionsHeader = false;
                }
            });
        }
        /// <summary>
        /// 使用 IAntiforgery  配置 antiforgery  功能
        /// </summary>
        /// <param name="app"></param>
        /// <param name="antiforgery"></param>
        public void Configure(IApplicationBuilder app, IAntiforgery antiforgery)
        {
            Check.NotNull(app, nameof(app));
            Check.NotNull(antiforgery, nameof(antiforgery));

            app.Use(next => context =>
            {
                string path = context.Request.Path.Value;
                if (string.Equals(path, "/", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(path, "/index.html", StringComparison.OrdinalIgnoreCase))
                {
                    // The request token can be sent as a JavaScript-readable cookie
                    var RequestToken = context.GetAntiXsrfRequestToken(antiforgery);
                    context.Response.Cookies.Append(Name, RequestToken,
                        new CookieOptions() { HttpOnly = false });
                }
                return next(context);
            });
        }

        private void RequestVerificationToken()
        {
        }

        #region internal var

        internal IConfiguration Configuration { get; }
        internal string AUTH { get; }
        internal string HOST { get; }
        internal string Name { get; }
        #endregion
    }

    public static class AntiforgeryExtensions
    {
        /// <summary>
        /// 获取 防止跨站点请求伪造 Token : Client XMLHttpRequest setRequestHeader("X-CSRF-TOKEN", from Server:Cookies["CSRF-TOKEN"])
        /// HTML: input type="hidden" id="__RequestVerificationToken" name="__RequestVerificationToken" value="@Context.GetAntiXsrfRequestToken(Xsrf)"
        /// </summary>
        /// <param name="context"></param>
        /// <param name="antiforgery">@inject Microsoft.AspNetCore.Antiforgery.IAntiforgery Xsrf</param>
        /// <returns></returns>
        public static string GetAntiXsrfRequestToken(this HttpContext Context, IAntiforgery Xsrf)
        {
            var tokens = Xsrf.GetAndStoreTokens(Context);
            return tokens.RequestToken;
        }
    }
}
