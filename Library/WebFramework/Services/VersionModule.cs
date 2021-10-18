using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using WebCore;

namespace WebFramework.Services
{
    /* appsettings.json
      "Version": "yes"
    */

    /// <summary>
    /// ApiVersionService Provides the implementation for ASP.NET Core.
    /// https://github.com/microsoft/aspnet-api-versioning/tree/master/samples/aspnetcore/SwaggerSample
    /// </summary>
    public static class ApiVersionService
    {
        /// <summary>
        /// Configuration in appsettings.json for UseVersionedApiExplorer,
        /// the Controller Attributes as follows,
        /// "Version": "yes" => [ApiVersion("1.0")] [Route("api/[controller]/v{version:apiVersion}/[action]")]
        /// "Version": "no" => [ApiExplorerSettings(GroupName = "demo"), Display(Name = "演示系统", Description = "演示系统描述文字")] [Route("api/[controller]/v1/[action]")]
        /// </summary>
        public const string AppSettings = "Version";
        /// <summary></summary>
        public const string HeaderApiVersion = "X-Api-Version";
        /// <summary></summary>
        public const string DefaultQueryParameterName = "version";
        /// <summary>select a definition in SwaggerUI</summary>
        public const string SelectDefinitionPrefix = "版本";
        /// <summary>false, if "Version" in appsettings.json is not yes</summary>
        internal static bool UseVersionedApiExplorer = false;
        /// <summary>please do not modify the variable</summary>
        internal static string DefaultVersion = "v1";
        /// <summary></summary>
        internal static ApiVersion DefaultApiVersion = ApiVersion.Default;
        /// <summary></summary>
        internal static readonly string[] ControllerBaseClasses = new string[] { "ControllerBase" };
        /// <summary></summary>
        internal const string controller = "Controller", controllerRoute = "[controller]";
        /// <summary>
        /// Api Controllers and Attributes.
        /// </summary>
        public static readonly Dictionary<string, SwaggerApiController> Controllers = new Dictionary<string, SwaggerApiController>();
        /// <summary>
        /// Api Versions for all functional module.
        /// </summary>
        public static readonly Dictionary<string, SwaggerApiVersion> Versions = new Dictionary<string, SwaggerApiVersion>();

        /// <summary>
        /// Init Api Controllers for all functional module.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="controllerBaseClasses"></param>
        static void Init(IConfiguration config, params string[] controllerBaseClasses)
        {
            var excludes = new List<string> { controller };
            if (controllerBaseClasses.Length == 0) controllerBaseClasses = ControllerBaseClasses;
            excludes.AddRange(controllerBaseClasses.Where(c => !string.IsNullOrEmpty(c)));
            var controllers = Assemblies.All.GetTypesOf<ControllerBase>(excludes.ToArray()).Where(c => c.GetCustomAttribute<ApiControllerAttribute>() != null);
            foreach (Type type in controllers)
            {
                var c = new SwaggerApiController
                {
                    Type = type,
                    Route = type.GetCustomAttribute<RouteAttribute>(),
                    ApiVersion = (Attribute.GetCustomAttributes(type, typeof(ApiVersionAttribute)) is ApiVersionAttribute[] attributes ? attributes : new ApiVersionAttribute[0]).Distinct().OrderBy(i => i.ToString()).ToArray(),
                    ApiVersions = new Dictionary<string, ApiVersionAttribute[]>(),
                    ApiExplorerSettings = type.GetCustomAttribute<ApiExplorerSettingsAttribute>(),
                    Display = type.GetCustomAttribute<DisplayAttribute>()
                };
                var v = new List<ApiVersion>();
                string k = DefaultVersion.ToLower(), name = null, description = null;
                if (c.ApiExplorerSettings != null && !string.IsNullOrWhiteSpace(c.ApiExplorerSettings.GroupName))
                {
                    name = c.ApiExplorerSettings.GroupName;
                    k = (ToolGood.Words.WordsHelper.HasChinese(name) ? ToolGood.Words.WordsHelper.GetPinyin(name) : name).Trim().Replace(" ", "");
                }
                if (c.Display != null)
                {
                    if (!string.IsNullOrWhiteSpace(c.Display.Name)) name = c.Display.Name;
                    if (!string.IsNullOrWhiteSpace(c.Display.GroupName)) name = c.Display.GroupName;
                    if (!string.IsNullOrWhiteSpace(c.Display.Description)) description = c.Display.Description;
                }
                if (Versions.ContainsKey(k)) v.AddRange(Versions[k].Versions);
                else Versions.Add(k, new SwaggerApiVersion() { Controllers = new Dictionary<string, SwaggerApiController>() });
                foreach (var ver in c.ApiVersion) v.AddRange(ver.Versions.Where(i => !v.Contains(i)));
                Versions[k].Module = k;
                Versions[k].Name = string.IsNullOrEmpty(name) ? SelectDefinitionPrefix + k : name;
                Versions[k].Description = description;
                Versions[k].Versions = v.Distinct().OrderBy(i => i.ToString()).ToArray();
                foreach (var p in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (p.Name.Equals(type.Name) || p.GetCustomAttribute<NonActionAttribute>() != null || c.ApiVersions.ContainsKey(p.Name)) continue;
                    if (Attribute.GetCustomAttributes(p, typeof(ApiVersionAttribute)) is not ApiVersionAttribute[] attributesInMethod)
                        attributesInMethod = c.ApiVersion.Concat(new ApiVersionAttribute[] { new ApiVersionAttribute(DefaultApiVersion.ToString()) }).ToArray();
                    if (p.Name.Equals("get_HttpContext")) break;
                    c.ApiVersions.Add(p.Name, attributesInMethod.Distinct().OrderBy(i => i.ToString()).ToArray());
                }
                Versions[k].Controllers.Add(c.Type.FullName, c);
                Controllers.Add(c.Type.FullName, c);
            }
            foreach (string k in Versions.Keys)
            {
                if (Versions[k].Versions.Length > 0) continue;
                Versions[k].Versions = new ApiVersion[] { ApiVersion.Default };
            }
            if (Versions.Count == 0) Versions.Add(DefaultVersion, new SwaggerApiVersion { Module = DefaultVersion.ToLower(), Name = SelectDefinitionPrefix + DefaultVersion, Versions = new ApiVersion[] { ApiVersion.Default }, Controllers = Controllers });
            var defaultApiVersion = config.GetSection(AppSettings)?.Value;
            UseVersionedApiExplorer = !string.IsNullOrEmpty(defaultApiVersion) && new[] { "no", "not", "null", "false", "0" }.Any(i => i.Equals(defaultApiVersion, StringComparison.OrdinalIgnoreCase)) == false;
            //if (UseVersionedApiExplorer) DefaultVersion = defaultApiVersion;
            if (!Versions.ContainsKey(DefaultVersion)) DefaultVersion = Versions.Keys.OrderBy(v => v).Last();
            DefaultApiVersion = Versions[DefaultVersion].Versions.OrderBy(v => v.ToString()).Last();
        }

        /// <summary>
        /// Adds api versions services to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection AddApiVersionService(this IServiceCollection services, IConfiguration config)
        {
            Init(config);

            if (!UseVersionedApiExplorer)
            {
                services.TryAddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();
                return services;
            }

            services.AddApiVersioning(options =>
            {
                options.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader(DefaultQueryParameterName), new UrlSegmentApiVersionReader());
                //options.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader(DefaultQueryParameterName), new UrlSegmentApiVersionReader(), new HeaderApiVersionReader() { HeaderNames = { HeaderApiVersion } });
                options.AssumeDefaultVersionWhenUnspecified = true;
                //options.ReportApiVersions = true; // headers "api-supported-versions" and "api-deprecated-versions"
                //options.DefaultApiVersion = DefaultApiVersion;
                options.RouteConstraintName = DefaultQueryParameterName;
            });

            services.AddVersionedApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = $"'v'VVV";
                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }
    }

    /// <summary>
    /// Swagger Doc ApiVersion and ApiExplorerSettings.
    /// [ApiExplorerSettings(GroupName = "demo"), Display(Name = "演示系统", Description = "演示系统描述文字")]
    /// </summary>
    public class SwaggerApiVersion
    {
        /// <summary>
        /// ApiExplorerSettings.GroupName => /swagger/{version}/swagger.json
        /// for functional module endpoint
        /// </summary>
        public string Module { get; set; }
        /// <summary>
        /// Display.Name => Swagger Endpoint Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Display.Description => Swagger Endpoint Description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// ApiVersion.Versions => Swagger Endpoint Versions
        /// </summary>
        public ApiVersion[] Versions { get; set; }
        /// <summary>
        /// All Api Controllers in functional module
        /// </summary>
        public Dictionary<string, SwaggerApiController> Controllers { get; set; }
    }

    /// <summary>
    /// Swagger Doc Api Controller and Attributes.
    /// </summary>
    public class SwaggerApiController
    {
        /// <summary>
        /// Api Controller Type extends ControllerBase with a [ApiController] Attribute
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// Api Route Attribute for Controller
        /// </summary>
        public RouteAttribute Route { get; set; }
        /// <summary>
        /// Api Version Attribute for Controller
        /// </summary>
        public ApiVersionAttribute[] ApiVersion { get; set; }
        /// <summary>
        /// Api Version Attributes for Actions in Controller Methods
        /// </summary>
        public Dictionary<string, ApiVersionAttribute[]> ApiVersions { get; set; }
        /// <summary>
        /// Api Explorer Settings Attribute for Controller
        /// </summary>
        public ApiExplorerSettingsAttribute ApiExplorerSettings { get; set; }
        /// <summary>
        /// Api Controller Display Attribute for Controller
        /// </summary>
        public DisplayAttribute Display { get; set; }
        /// <summary>
        /// Get Api Versions
        /// </summary>
        /// <param name="actionInControllerMethod"></param>
        /// <param name="addControllerVersions"></param>
        /// <returns></returns>
        public ApiVersion[] Versions(string actionInControllerMethod = null, bool addControllerVersions = true)
        {
            var list = new List<ApiVersion>();
            if (addControllerVersions || string.IsNullOrEmpty(actionInControllerMethod))
            {
                foreach (var v in ApiVersion) list.AddRange(v.Versions);
            }
            if (actionInControllerMethod != null && ApiVersions.ContainsKey(actionInControllerMethod))
            {
                foreach (var v in ApiVersions[actionInControllerMethod]) list.AddRange(v.Versions);
            }
            return list.Distinct().OrderBy(v => v.ToString()).ToArray();
        }
    }
}
