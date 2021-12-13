using System;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Swagger: 请求头，用于标识接口请求头参数信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequestHeaderAttribute : Attribute
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 是否必填项
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// 初始化一个<see cref="RequestHeaderAttribute"/>类型的实例
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="description">备注</param>
        public RequestHeaderAttribute(string name, string description = null)
        {
            Name = name;
            Description = description;
        }
    }
}
