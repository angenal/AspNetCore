using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using WebCore.Attributes;

namespace WebFramework.Extensions
{
    /// <summary>
    /// 类型-属性-反射
    /// </summary>
    public static class Attr<TModel>
    {
        public static RequiredAttribute RequiredFor<TResult>(Expression<Func<TModel, TResult>> expression)
        {
            return RequiredFor(PropertyName(expression));
        }
        public static RequiredAttribute RequiredFor(string propertyName)
        {
            if (propertyName != null)
            {
                MemberInfo m = typeof(TModel).GetProperty(propertyName);
                if (m != null && m.GetCustomAttribute(typeof(RequiredAttribute)) is RequiredAttribute a)
                    return a;
            }
            return null;
        }

        /// <summary>
        /// 获取Model自定义DisplayAttribute的Name
        /// </summary>
        public static DisplayAttribute DisplayNameFor<TResult>(Expression<Func<TModel, TResult>> expression)
        {
            return DisplayNameFor(PropertyName(expression));
        }
        public static DisplayAttribute DisplayNameFor(string propertyName)
        {
            if (propertyName != null)
            {
                MemberInfo m = typeof(TModel).GetProperty(propertyName);
                if (m != null && m.GetCustomAttribute(typeof(DisplayAttribute)) is DisplayAttribute a)
                    return a;
            }
            return null;
        }


        public static StringLengthAttribute StringLengthFor<TResult>(Expression<Func<TModel, TResult>> expression)
        {
            return StringLengthFor(PropertyName(expression));
        }
        public static StringLengthAttribute StringLengthFor(string propertyName)
        {
            if (propertyName != null)
            {
                MemberInfo m = typeof(TModel).GetProperty(propertyName);
                if (m != null && m.GetCustomAttribute(typeof(StringLengthAttribute)) is StringLengthAttribute a)
                    return a;
            }
            return null;
        }

        public static FileSizeAttribute FileSizeFor<TResult>(Expression<Func<TModel, TResult>> expression)
        {
            return FileSizeFor(PropertyName(expression));
        }
        public static FileSizeAttribute FileSizeFor(string propertyName)
        {
            if (propertyName != null)
            {
                MemberInfo m = typeof(TModel).GetProperty(propertyName);
                if (m != null && m.GetCustomAttribute(typeof(FileSizeAttribute)) is FileSizeAttribute a)
                    return a;
            }
            return null;
        }


        private static string PropertyName<TResult>(Expression<Func<TModel, TResult>> expression)
        {
            if (expression.Body.NodeType.Equals(ExpressionType.MemberAccess))
                return expression.ToString().Split('.').LastOrDefault();
            return null;
        }
    }
}
