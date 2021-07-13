using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public static IServiceCollection RegisterResources(this IServiceCollection services, IConfiguration config)
        {
            var section = config.GetSection(LanguageRouteConstraint.AppSettings);
            if (!section.Exists()) return services;

            var culture = section.Value;
            var cultures = Localizations.SupportedCultures();
            LanguageRouteConstraint.Cultures = cultures.Select(c => c.Name);
            services.AddLocalization(options => options.ResourcesPath = ResourcesPath);
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(culture: culture, uiCulture: culture);
                options.SupportedCultures = cultures;
                options.SupportedUICultures = options.SupportedCultures;
                options.RequestCultureProviders = new IRequestCultureProvider[] { new RouteDataRequestCultureProvider() };
            });
            services.Configure<RouteOptions>(options => options.ConstraintMap.Add(LanguageRouteConstraint.Key, typeof(LanguageRouteConstraint)));

            return services;
        }
    }

    public class LanguageRouteConstraint : IRouteConstraint
    {
        /// <summary>
        /// Configuration "Culture" in appsettings.json
        /// </summary>
        public static string AppSettings => Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Key);

        public const string Key = "culture";

        internal static IEnumerable<string> Cultures = Array.Empty<string>();

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (!values.ContainsKey(Key)) return false;
            var culture = values[Key].ToString();
            return Cultures.Contains(culture);
        }
    }

    public class RouteDataRequestCultureProvider : RequestCultureProvider
    {
        public int IndexOfCulture { get; set; } = (int)Localizations.Default;
        public int IndexofUICulture { get; set; } = (int)Localizations.Default;

        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var culture = httpContext.Request.Path.Value.Split('/')[IndexOfCulture]?.ToString();
            var providerResultCulture = new ProviderCultureResult(culture, culture);

            return Task.FromResult(providerResultCulture);
        }
    }
}
