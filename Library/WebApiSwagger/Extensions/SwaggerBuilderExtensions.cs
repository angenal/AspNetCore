using Microsoft.AspNetCore.Builder;
using System;
using WebApiSwagger.Core.Authorization;
using WebApiSwagger.Internals;

// ReSharper disable once CheckNamespace
namespace WebApiSwagger
{
    /// <summary>
    /// Swagger 文档构建工具
    /// </summary>
    public static class SwaggerBuilderExtensions
    {
        /// <summary>
        /// 启用Swagger
        /// </summary>
        /// <param name="app">应用构建器</param>
        /// <param name="setupAction">配置操作</param>
        public static IApplicationBuilder UseSwaggerDoc(this IApplicationBuilder app, Action<SwaggerDocOptions> setupAction = null)
        {
            BuildContext.Instance.ServiceProvider = app.ApplicationServices;
            setupAction?.Invoke(BuildContext.Instance.ExOptions);
            if (BuildContext.Instance.ExOptions.EnableAuthorization()) app.UseMiddleware<SwaggerAuthorizeMiddleware>();
            app.UseSwagger(o => BuildContext.Instance.ExOptions.InitSwaggerOptions(o)).UseSwaggerUI(o => BuildContext.Instance.ExOptions.InitSwaggerUiOptions(o));
            return app;
        }
    }
}
