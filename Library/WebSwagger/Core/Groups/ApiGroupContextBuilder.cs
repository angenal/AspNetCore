using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WebSwagger.Internals;

namespace WebSwagger.Core.Groups
{
    /// <summary>
    /// Api分组上下文构建器
    /// </summary>
    internal class ApiGroupContextBuilder
    {
        /// <summary>
        /// 构建上下文
        /// </summary>
        public ApiGroupContext Build()
        {
            var context = new ApiGroupContext();
            var buildContext = BuildContext.Instance;
            BuildGroup(context, buildContext);
            BuildCustomVersion(context, buildContext);
            BuildNoGroup(context, buildContext);
            BuildApiVersion(context, buildContext);
            return context;
        }

        /// <summary>
        /// 构建分组
        /// </summary>
        /// <param name="context">Api分组上下文</param>
        /// <param name="buildContext">构建上下文</param>
        private void BuildGroup(ApiGroupContext context, BuildContext buildContext)
        {
            if (!buildContext.DocOptions.EnableApiGroup())
                return;
            if (!buildContext.DocOptions.ApiGroupType.IsEnum)
                return;
            buildContext.DocOptions.ApiGroupType.GetFields().Skip(1).ToList().ForEach(x =>
            {
                var attribute = x.GetCustomAttributes(typeof(DisplayAttribute), false).OfType<DisplayAttribute>().FirstOrDefault();
                if (attribute == null)
                {
                    if (buildContext.DocOptions.EnableApiVersion)
                    {
                        context.AddGroup(x.Name);
                        return;
                    }
                    context.AddApiGroupByCustomGroup(attribute.Name, x.Name, attribute.Description, x.Name, ApiVersion.DefaultGroupName);
                    return;
                }

                if (buildContext.DocOptions.EnableApiVersion)
                {
                    context.AddGroup(attribute.Name, x.Name, attribute.Description);
                    return;
                }
                context.AddApiGroupByCustomGroup(attribute.Name, x.Name, attribute.Description, x.Name, ApiVersion.DefaultGroupName);
            });
        }

        /// <summary>
        /// 构建自定义版本
        /// </summary>
        /// <param name="context">Api分组上下文</param>
        /// <param name="buildContext">构建上下文</param>
        private void BuildCustomVersion(ApiGroupContext context, BuildContext buildContext)
        {
            if (buildContext.DocOptions.EnableApiGroup() || buildContext.DocOptions.EnableApiVersion)
                return;
            if (!buildContext.DocOptions.HasCustomVersion())
                return;
            foreach (var apiVersion in buildContext.DocOptions.ApiVersions)
                context.AddApiGroup(apiVersion.Version, apiVersion.Description);
        }

        /// <summary>
        /// 构建API多版本
        /// </summary>
        /// <param name="context">Api分组上下文</param>
        /// <param name="buildContext">构建上下文</param>
        private void BuildApiVersion(ApiGroupContext context, BuildContext buildContext)
        {
            if (!buildContext.DocOptions.EnableApiVersion)
                return;
            var provider = buildContext.ServiceProvider.GetService<IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions)
            {
                if (buildContext.DocOptions.EnableApiGroup())
                {
                    context.AddApiVersion(description.GroupName, description.ApiVersion.ToString());
                    continue;
                }
                context.AddApiGroup(buildContext.DocOptions.SwaggerUiOptions.DocumentTitle,
                    description.GroupName, string.Empty, description.GroupName, description.ApiVersion.ToString());
            }
        }

        /// <summary>
        /// 构建未分组
        /// </summary>
        /// <param name="context">Api分组上下文</param>
        /// <param name="buildContext">构建上下文</param>
        private void BuildNoGroup(ApiGroupContext context, BuildContext buildContext)
        {
            if (!buildContext.DocOptions.EnableApiGroup())
                return;
            if (buildContext.DocOptions.EnableApiVersion)
                context.AddNoGroup();
            else
                context.AddNoGroupWithVersion();
        }
    }
}
