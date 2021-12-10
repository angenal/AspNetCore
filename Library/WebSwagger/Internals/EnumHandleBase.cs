using System;
using System.Text;

namespace WebSwagger.Internals
{
    /// <summary>
    /// 枚举处理基类
    /// </summary>
    internal abstract class EnumHandleBase
    {
        /// <summary>
        /// 格式化描述
        /// </summary>
        /// <param name="type">枚举类型</param>
        protected virtual string FormatDescription(Type type)
        {
            var sb = new StringBuilder();
            var result = type.GetEnumDescriptions();
            foreach (var item in result)
                sb.Append($"{item.Value} = {(string.IsNullOrEmpty(item.Description) ? item.Name : item.Description)}{Environment.NewLine}");
            return sb.ToString();
        }
    }
}
