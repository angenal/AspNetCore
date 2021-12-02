using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace WebSwagger
{
    /// <summary>
    /// Swagger 接口文档选项配置 扩展
    /// </summary>
    internal static class SwaggerDocOptionsExtensions
    {
        /// <summary>
        /// 是否启用授权
        /// </summary>
        /// <param name="options">Swagger 接口文档选项配置</param>
        public static bool EnableAuthorization(this SwaggerDocOptions options) => options.SwaggerAuthorizations.Any();

        /// <summary>
        /// 是否启用API分组
        /// </summary>
        /// <param name="options">Swagger 接口文档选项配置</param>
        public static bool EnableApiGroup(this SwaggerDocOptions options) => options.ApiGroupType != null;

        /// <summary>
        /// 是否自定义版本
        /// </summary>
        /// <param name="options">Swagger 接口文档选项配置</param>
        public static bool HasCustomVersion(this SwaggerDocOptions options) => options.ApiVersions.Any();

        /// <summary>
        /// 初始化Swagger生成选项配置
        /// </summary>
        /// <param name="options">Swagger 接口文档选项配置</param>
        /// <param name="swaggerGenOptions">Swagger 生成选项配置</param>
        public static void InitSwaggerGenOptions(this SwaggerDocOptions options, SwaggerGenOptions swaggerGenOptions)
        {
            options.SwaggerGenOptions = swaggerGenOptions;
            options.AddSwaggerGenAction?.Invoke(swaggerGenOptions);
        }

        /// <summary>
        /// 初始化Swagger选项配置
        /// </summary>
        /// <param name="options">Swagger 接口文档选项配置</param>
        /// <param name="swaggerOptions">Swagger 选项配置</param>
        public static void InitSwaggerOptions(this SwaggerDocOptions options, SwaggerOptions swaggerOptions)
        {
            options.SwaggerOptions = swaggerOptions;
            options.UseSwaggerAction?.Invoke(swaggerOptions);
        }
    }
}
