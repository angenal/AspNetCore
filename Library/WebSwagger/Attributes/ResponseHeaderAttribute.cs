using System;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Swagger: 响应请求头，用于标识接口响应请求头参数信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ResponseHeaderAttribute : Attribute
    {
        /// <summary>
        /// 初始化一个<see cref="ResponseHeaderAttribute"/>类型的实例
        /// </summary>
        /// <param name="statusCode">状态码</param>
        /// <param name="name">名称</param>
        /// <param name="description">描述</param>
        /// <param name="type">类型</param>
        /// <param name="format">格式化</param>
        public ResponseHeaderAttribute(int statusCode, string name, string description, string type, string format = "")
        {
            StatusCodes = new[] { statusCode };
            Name = name;
            Description = description;
            Type = type;
            Format = format;
        }

        /// <summary>
        /// 初始化一个<see cref="ResponseHeaderAttribute"/>类型的实例
        /// </summary>
        /// <param name="statusCode1">状态码1</param>
        /// <param name="statusCode2">状态码2</param>
        /// <param name="name">名称</param>
        /// <param name="description">描述</param>
        /// <param name="type">类型</param>
        /// <param name="format">格式化</param>
        public ResponseHeaderAttribute(int statusCode1, int statusCode2, string name, string description, string type, string format = "")
        {
            StatusCodes = new[] { statusCode1, statusCode2 };
            Name = name;
            Description = description;
            Type = type;
            Format = format;
        }

        /// <summary>
        /// 初始化一个<see cref="ResponseHeaderAttribute"/>类型的实例
        /// </summary>
        /// <param name="statusCode">状态码</param>
        /// <param name="name">名称</param>
        /// <param name="description">描述</param>
        public ResponseHeaderAttribute(int statusCode, string name, string description)
        {
            StatusCodes = new[] { statusCode };
            Name = name;
            Description = description;
        }

        /// <summary>
        /// 初始化一个<see cref="ResponseHeaderAttribute"/>类型的实例
        /// </summary>
        /// <param name="statusCode1">状态码1</param>
        /// <param name="statusCode2">状态码2</param>
        /// <param name="name">名称</param>
        /// <param name="description">描述</param>
        public ResponseHeaderAttribute(int statusCode1, int statusCode2, string name, string description)
        {
            StatusCodes = new[] { statusCode1, statusCode2 };
            Name = name;
            Description = description;
        }

        /// <summary>
        /// 初始化一个<see cref="ResponseHeaderAttribute"/>类型的实例
        /// </summary>
        /// <param name="statusCode">状态码</param>
        /// <param name="name">名称</param>
        /// <param name="description">描述</param>
        /// <param name="type">类型</param>
        /// <param name="format">格式化</param>
        public ResponseHeaderAttribute(int[] statusCode, string name, string description, string type, string format = "")
        {
            StatusCodes = statusCode;
            Name = name;
            Description = description;
            Type = type;
            Format = format;
        }

        /// <summary>
        /// 初始化一个<see cref="ResponseHeaderAttribute"/>类型的实例
        /// </summary>
        /// <param name="statusCode">状态码</param>
        /// <param name="name">名称</param>
        /// <param name="type">类型</param>
        /// <param name="description">描述</param>
        /// <param name="format">格式化</param>
        public ResponseHeaderAttribute(int[] statusCode, string name, string description)
        {
            StatusCodes = statusCode;
            Name = name;
            Description = description;
        }

        /// <summary>
        /// 初始化一个<see cref="ResponseHeaderAttribute"/>类型的实例
        /// </summary>
        /// <param name="statusCode">状态码</param>
        public ResponseHeaderAttribute(params int[] statusCode)
        {
            StatusCodes = statusCode;
        }

        /// <summary>
        /// 状态码
        /// </summary>
        public int[] StatusCodes { get; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 格式化
        /// </summary>
        public string Format { get; }
    }
}
