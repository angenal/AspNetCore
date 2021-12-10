using EnumsNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WebSwagger.Internals;

namespace WebSwagger
{
    /// <summary>
    /// 枚举功能扩展
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 获取描述列表
        /// </summary>
        /// <param name="type">枚举类型</param>
        public static List<(int Value, string Name, string Description)> GetEnumDescriptions(this Type type)
        {
            if (InternalCache.EnumDict.ContainsKey(type))
                return InternalCache.EnumDict[type];
            var result = new List<(int Value, string Name, string Description)>();
            if (!type.IsEnum)
                return result;
            foreach (var member in Enums.GetMembers(type))
            {
                var attribute = member.Attributes.FirstOrDefault(t => t is DescriptionAttribute);
                var description = attribute == null ? member.Name : ((DescriptionAttribute)attribute).Description;
                result.Add(((int)member.Value, member.Name, description));
            }
            InternalCache.EnumDict[type] = result;
            return result;
        }
    }
}
