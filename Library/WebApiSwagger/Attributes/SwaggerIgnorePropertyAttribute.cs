using System;

namespace WebApiSwagger.Attributes
{
    /// <summary>
    /// Swagger: 隐藏属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SwaggerIgnorePropertyAttribute : Attribute
    {
    }
}
