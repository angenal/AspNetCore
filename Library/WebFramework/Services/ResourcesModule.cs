using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WebCore;
using WebInterface;

namespace WebFramework.Services
{
    /// <summary>
    /// Register i18n supports multi language
    /// </summary>
    public static class RegisterResourcesModule
    {
        /// <summary>
        /// Application Resources Path
        /// /Resources/Controllers/{controller}.en-US.resx {controller}.en-US.Designer.cs
        /// /Resources/Controllers/{controller}.zh-CN.resx ...
        /// </summary>
        public const string ResourcesPath = "Resources";

        /// <summary></summary>
        public static IServiceCollection AddResources(this IServiceCollection services, IConfiguration config, IMvcBuilder builder)
        {
            var section = config.GetSection(LanguageRouteConstraint.AppSettings);
            var culture = section.Exists() ? section.Value : Localizations.Default.ToDescription(false);
            var cultures = Localizations.SupportedCultures();
            Localizations.DefaultCulture = culture;
            LanguageRouteConstraint.SupportedCultures = cultures;
            LanguageRouteConstraint.Cultures = cultures.Select(c => c.Name).ToArray();
            LanguageRouteConstraint.Languages = Enum.GetNames(typeof(Language));
            // 注册本地化服务
            services.AddLocalization(options => options.ResourcesPath = ResourcesPath);
            // 注册视图本地化服务 Microsoft.AspNetCore.Mvc.Razor
            //builder.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix, options => { options.ResourcesPath = ResourcesPath; });
            // 注册数据注解本地化服务
            builder.AddDataAnnotationsLocalization();
            // 配置本地化服务
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(culture: culture, uiCulture: culture);
                options.RequestCultureProviders = new IRequestCultureProvider[] { new RouteDataRequestCultureProvider() };
                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;
            });
            // 配置请求路由服务
            services.Configure<RouteOptions>(options => options.ConstraintMap.Add(LanguageRouteConstraint.Key, typeof(LanguageRouteConstraint)));

            return services;
        }

        /// <summary></summary>
        public static IApplicationBuilder UseResources(this IApplicationBuilder app)
        {
            app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>()?.Value ?? new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(Localizations.DefaultCulture),
                SupportedCultures = LanguageRouteConstraint.SupportedCultures,
                SupportedUICultures = LanguageRouteConstraint.SupportedCultures
            });

            return app;
        }
    }

    /// <summary></summary>
    public class LanguageRouteConstraint : IRouteConstraint
    {
        /// <summary>
        /// Configuration "Culture" in appsettings.json
        /// </summary>
        public const string AppSettings = "Culture"; // Key.ToTitleCase();

        /// <summary></summary>
        public const string Key = "culture";

        /// <summary></summary>
        internal static IList<CultureInfo> SupportedCultures = new List<CultureInfo>();
        /// <summary></summary>
        internal static IEnumerable<string> Cultures = Array.Empty<string>();
        /// <summary></summary>
        internal static IEnumerable<string> Languages = Array.Empty<string>();

        /// <summary></summary>
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values.ContainsKey(Key)) return Cultures.Contains(values[Key].ToString());
            return httpContext.Request.Query.TryGetValue(Key, out var value) && Cultures.Contains(value.ToString());
        }
    }

    /// <summary></summary>
    public class RouteDataRequestCultureProvider : RequestCultureProvider
    {
        /// <summary></summary>
        public int IndexOfCulture { get; set; } = (int)Localizations.Default;
        /// <summary></summary>
        public int IndexofUICulture { get; set; } = (int)Localizations.Default;

        /// <summary></summary>
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var culture = httpContext.Request.Path.Value.Split('/')[IndexOfCulture]?.ToString();
            var providerResultCulture = new ProviderCultureResult(culture, culture);

            return Task.FromResult(providerResultCulture);
        }
    }
}
