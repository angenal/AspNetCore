using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCore;

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
            LanguageRouteConstraint.Cultures = cultures.Select(c => c.Name);
            services.AddLocalization(options => options.ResourcesPath = ResourcesPath);
            builder.AddDataAnnotationsLocalization();
            //builder.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix, options => { options.ResourcesPath = ResourcesPath; }); // Microsoft.AspNetCore.Mvc.Razor
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(culture: culture, uiCulture: culture);
                options.RequestCultureProviders = new IRequestCultureProvider[] { new RouteDataRequestCultureProvider() };
                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;
            });
            services.Configure<RouteOptions>(options => options.ConstraintMap.Add(LanguageRouteConstraint.Key, typeof(LanguageRouteConstraint)));

            return services;
        }
    }

    /// <summary></summary>
    public class LanguageRouteConstraint : IRouteConstraint
    {
        /// <summary>
        /// Configuration "Culture" in appsettings.json
        /// </summary>
        public const string AppSettings = "Culture"; // Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Key);

        /// <summary></summary>
        public const string Key = "culture";

        /// <summary></summary>
        internal static IEnumerable<string> Cultures = Array.Empty<string>();

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
