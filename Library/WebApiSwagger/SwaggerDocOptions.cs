using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using WebApiSwagger.Core.Authorization;

namespace WebApiSwagger
{
    /// <summary>
    /// Swagger 接口文档选项配置
    /// </summary>
    public class SwaggerDocOptions
    {
        /// <summary>
        /// 初始化一个<see cref="SwaggerDocOptions"/>类型的实例
        /// </summary>
        public SwaggerDocOptions() { }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; } = "REST API";

        /// <summary>
        /// 路由前缀。默认：swagger
        /// </summary>
        public string RoutePrefix { get; set; } = "swagger";

        /// <summary>
        /// Api版本列表
        /// </summary>
        public List<ApiVersion> ApiVersions { get; set; } = new List<ApiVersion>();

        /// <summary>
        /// 是否启用自定义首页
        /// </summary>
        public bool EnableCustomIndex { get; set; }

        /// <summary>
        /// 是否启用缓存
        /// </summary>
        public bool EnableCached { get; set; }

        /// <summary>
        /// 是否启用API版本号
        /// </summary>
        public bool EnableApiVersion { get; set; }

        /// <summary>
        /// Api分组类型
        /// </summary>
        public Type ApiGroupType { get; set; }

        /// <summary>
        /// Swagger授权登录账号，未指定则不启用
        /// </summary>
        public List<SwaggerAuthorizationUser> SwaggerAuthorizations { get; set; } = new List<SwaggerAuthorizationUser>();

        /// <summary>
        /// UseSwagger 操作
        /// </summary>
        public Action<SwaggerOptions> UseSwaggerAction { get; set; }

        /// <summary>
        /// UseSwaggerUI 操作
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public Action<SwaggerUIOptions> UseSwaggerUIAction { get; set; }

        /// <summary>
        /// AddSwaggerGen 操作
        /// </summary>
        public Action<SwaggerGenOptions> AddSwaggerGenAction { get; set; }

        /// <summary>
        /// Swagger选项配置
        /// </summary>
        internal SwaggerOptions SwaggerOptions { get; set; }

        /// <summary>
        /// SwaggerUI选项配置
        /// </summary>
        internal SwaggerUIOptions SwaggerUiOptions { get; set; }

        /// <summary>
        /// Swagger选项配置
        /// </summary>
        internal SwaggerGenOptions SwaggerGenOptions { get; set; }
    }

    /// <summary>
    /// Api版本
    /// </summary>
    public class ApiVersion
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// 令牌存储参数。用于拦截登录后存储令牌，解决刷新页面导致令牌丢失问题
    /// </summary>
    public class TokenStorageParameter
    {
        /// <summary>
        /// 授权定义。对应于 AddSecurityDefinition 中的 name
        /// </summary>
        public string SecurityDefinition { get; set; }

        /// <summary>
        /// 网页缓存类型
        /// </summary>
        public WebCacheType CacheType { get; set; }
    }

    /// <summary>
    /// 网页缓存方式
    /// </summary>
    public enum WebCacheType
    {
        /// <summary>
        /// 本地存储
        /// </summary>
        Local,
        /// <summary>
        /// 会话
        /// </summary>
        Session,
    }
}
