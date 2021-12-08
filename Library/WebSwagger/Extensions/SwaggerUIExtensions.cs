using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Linq;
using WebSwagger.Internals;

// ReSharper disable once CheckNamespace
namespace WebSwagger
{
    /// <summary>
    /// SwaggerUI 扩展
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class SwaggerUIExtensions
    {
        /// <summary>
        /// 使用默认的UI
        /// </summary>
        /// <param name="options">SwaggerUI配置</param>
        // ReSharper disable once InconsistentNaming
        public static void UseDefaultSwaggerUI(this SwaggerUIOptions options)
        {
            options.DefaultModelExpandDepth(2);// 接口列表折叠配置
            options.DefaultModelRendering(ModelRendering.Example);// 控制首次呈现API时模型的显示方式（模型|示例）。
            options.ShowExtensions();// 显示扩展信息
            options.DefaultModelsExpandDepth(-1);// 隐藏model
            options.DisplayOperationId();// 显示控制器接口方法名
            options.DisplayRequestDuration();// 显示请求持续时间（以毫秒为单位）
            options.DocExpansion(DocExpansion.None);// 文档显示方式：显示控制器
            options.EnableDeepLinking();// 启用深层连接，用于指定Url自动跳转到相应标签
            options.EnableFilter();// 启用过滤文本框
        }

        /// <summary>
        /// 使用自定义首页
        /// </summary>
        /// <param name="options">SwaggerUI选项</param>
        public static void UseCustomSwaggerIndex(this SwaggerUIOptions options)
        {
            if (customSwaggerIndexPath != null) return;
            customSwaggerIndexPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "swagger", "index.html");

            options.IndexStream = () =>
            {
                if (File.Exists(customSwaggerIndexPath)) return File.OpenRead(customSwaggerIndexPath);
                return SwaggerDocServices.Assembly.GetManifestResourceStream($"WebSwagger.Resources.index.html");
            };
        }
        private static string customSwaggerIndexPath;

        /// <summary>
        /// 使用令牌存储。解决刷新页面导致令牌丢失问题，前提必须使用 <see cref="UseCustomSwaggerIndex"/> 方法
        /// </summary>
        /// <param name="options">SwaggerUI选项</param>
        /// <param name="securityDefinition">授权定义。对应于 AddSecurityDefinition 中的 name</param>
        /// <param name="securityScheme">授权方式</param>
        /// <param name="cacheType">缓存类型</param>
        public static void UseTokenStorage(this SwaggerUIOptions options, string securityDefinition, TokenDefinitionParameter securityScheme, WebCacheType cacheType = WebCacheType.Session)
        {
            options.ConfigObject.AdditionalItems[$"{securityDefinition}Storage"] = new TokenStorageParameter
            {
                CacheType = cacheType,
                SecurityDefinition = securityDefinition,
                SecurityScheme = securityScheme
            };
        }
        internal static TokenDefinitionParameter ConvertToTDP(this OpenApiSecurityScheme scheme)
        {
            return new TokenDefinitionParameter
            {
                Name = scheme.Name,
                Description = scheme.Description,
                In = scheme.In,
                Type = scheme.Type
            };
        }

        /// <summary>
        /// 使用内部资源。
        /// </summary>
        /// <param name="options">SwaggerUI选项</param>
        public static void UseInternalResources(this SwaggerUIOptions options)
        {
            options.InjectJavascript("https://cdn.bootcdn.net/ajax/libs/jquery/2.1.4/jquery.min.js");
            options.InjectJavascript("resources?name=jquery.initialize.min.js");
            //options.InjectJavascript("/swagger/resources?name=export.js");
            options.InjectStylesheet("resources?name=swagger-common.css");
        }

        /// <summary>
        /// 使用翻译。
        /// </summary>
        /// <param name="options">SwaggerUI选项</param>
        /// <param name="language">语言</param>
        public static void UseTranslate(this SwaggerUIOptions options, string language = "zh-cn")
        {
            options.InjectJavascript($"resources/getLanguage?name={language}");
            options.InjectJavascript("resources?name=translate.js");
        }

        /// <summary>
        /// 添加信息
        /// </summary>
        /// <param name="options">SwaggerUI选项配置</param>
        /// <param name="name">名称</param>
        /// <param name="url">地址</param>
        internal static void AddInfo(this SwaggerUIOptions options, string name, string url)
        {
            if (options.ExistsApiVersion(name, url))
                return;
            var urlMaps = BuildContext.Instance.GetUrlMaps();
            urlMaps[name] = url;
            options.SwaggerEndpoint(url, name);
        }

        /// <summary>
        /// 是否存在Api版本
        /// </summary>
        /// <param name="options">SwaggerUI选项</param>
        /// <param name="name">名称</param>
        /// <param name="url">地址</param>
        internal static bool ExistsApiVersion(this SwaggerUIOptions options, string name, string url)
        {
            if (options?.ConfigObject?.Urls == null)
                return false;
            return options.ConfigObject.Urls.Any(x => x.Name == name || x.Url == url);
        }
    }
}
