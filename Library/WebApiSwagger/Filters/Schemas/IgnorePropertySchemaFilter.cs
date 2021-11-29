using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;
using WebApiSwagger.Attributes;

namespace WebApiSwagger.Filters.Schemas
{
    /// <summary>
    /// 忽略属性 过滤器
    /// </summary>
    public class IgnorePropertySchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// 重写操作处理
        /// </summary>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null)
                return;
            var ignoreProperties = context.Type.GetProperties()
                .Where(t => t.GetCustomAttribute<SwaggerIgnorePropertyAttribute>() != null);
            foreach (var ignoreProperty in ignoreProperties)
            {
                var propertyToRemove =
                    schema.Properties.Keys.SingleOrDefault(x => x.ToLower() == ignoreProperty.Name.ToLower());
                if (propertyToRemove != null) 
                    schema.Properties.Remove(propertyToRemove);
            }
        }
    }
}
