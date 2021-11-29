using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace WebApiSwagger.Filters.Operations
{
    /// <summary>
    /// 添加 默认值 操作过滤器
    /// </summary>
    public class DefaultValueOperationFilter : IOperationFilter
    {
        /// <summary>
        /// 重写操作处理
        /// </summary>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            operation.Deprecated |= apiDescription.IsDeprecated();

            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
            foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
            {
                var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
                var response = operation.Responses[responseKey];

                foreach (var contentType in response.Content.Keys)
                {
                    if (!responseType.ApiResponseFormats.Any(x => x.MediaType == contentType))
                    {
                        response.Content.Remove(contentType);
                    }
                }
            }

            if (operation?.Parameters == null || !operation.Parameters.Any())
            {
                return;
            }

            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
            //foreach (var parameter in operation.Parameters)
            //{
            //    var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

            //    if (parameter.Description == null)
            //    {
            //        parameter.Description = description.ModelMetadata?.Description;
            //    }

            //    if (parameter.Schema.Default == null && description.DefaultValue != null)
            //    {
            //        var json = JsonSerializer.Serialize(description.DefaultValue, description.ModelMetadata.ModelType);
            //        parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(json);
            //    }

            //    parameter.Required |= description.IsRequired;
            //}

            var parameterValuePairs = context.ApiDescription.ParameterDescriptions
                .Where(parameter => GetDefaultValueAttribute(parameter) != null || HasDefaultValue(parameter))
                .ToDictionary(parameter => parameter.Name, GetDefaultValue);
            foreach (var parameter in operation.Parameters)
            {
                if (parameterValuePairs.TryGetValue(parameter.Name, out var defaultValue))
                {
                    if (defaultValue == null)
                        continue;
                    //if (parameter is NonBodyParameter nonBodyParameter)
                    //{
                    //    nonBodyParameter.Default = defaultValue;
                    //    nonBodyParameter.Required = false;
                    //}
                }
            }
        }

        /// <summary>
        /// 获取默认值特性
        /// </summary>
        /// <param name="parameter">API参数</param>
        private DefaultValueAttribute GetDefaultValueAttribute(ApiParameterDescription parameter)
        {
            if (!(parameter.ModelMetadata is DefaultModelMetadata metadata) || metadata.Attributes.PropertyAttributes == null)
                return null;
            return metadata.Attributes.PropertyAttributes.OfType<DefaultValueAttribute>().FirstOrDefault();
        }

        /// <summary>
        /// 获取参数信息
        /// </summary>
        /// <param name="parameter">API参数</param>
        private ParameterInfo GetParameterInfo(ApiParameterDescription parameter) => ((ControllerParameterDescriptor) parameter.ParameterDescriptor).ParameterInfo;

        /// <summary>
        /// 是否含有默认值
        /// </summary>
        /// <param name="parameter">API参数</param>
        private bool HasDefaultValue(ApiParameterDescription parameter)
        {
            if (parameter.ParameterDescriptor is ControllerParameterDescriptor controllerParameterDescriptor)
            {
                return controllerParameterDescriptor.ParameterInfo.HasDefaultValue;
            }
            return false;
        }

        /// <summary>
        /// 获取默认值
        /// </summary>
        /// <param name="parameter">API参数</param>
        private object GetDefaultValue(ApiParameterDescription parameter)
        {
            var parameterInfo = GetParameterInfo(parameter);
            if (parameterInfo.HasDefaultValue)
                return parameter.Type.IsEnum ? null : parameterInfo.DefaultValue;
            var defaultValueAttribute = GetDefaultValueAttribute(parameter);
            return defaultValueAttribute.Value;
        }
    }
}
