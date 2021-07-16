using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WebCore.Attributes
{
    /// <summary>
    /// 类型-属性-反射
    /// </summary>
    /// <typeparam name="TModel">实体类型</typeparam>
    public static class Attr<TModel>
    {
        /// <summary>
        /// 获取Model自定义属性T
        /// </summary>
        public static T Get<T, TResult>(Expression<Func<TModel, TResult>> expression, bool inherit = false) where T : Attribute
        {
            var propertyName = Name(expression);
            return Get<T>(propertyName, inherit);
        }
        /// <summary>
        /// 获取Model自定义属性T
        /// </summary>
        public static T Get<T>(string propertyName, bool inherit = false) where T : Attribute
        {
            if (propertyName != null)
            {
                MemberInfo m = typeof(TModel).GetProperty(propertyName);
                if (m != null && m.GetCustomAttribute(typeof(T), inherit) is T a)
                    return a;
            }
            return null;
        }
        /// <summary>
        /// 存在Model自定义属性T
        /// </summary>
        public static bool Exists<T, TResult>(Expression<Func<TModel, TResult>> expression, bool inherit = false) where T : Attribute
        {
            var propertyName = Name(expression);
            MemberInfo mInfo = typeof(TModel).GetProperty(propertyName);
            return mInfo != null && mInfo.GetCustomAttributes(typeof(T), inherit).Any(m => m as T != null);
        }

        public static PropertyInfo Get<TResult>(Expression<Func<TModel, TResult>> expression)
        {
            var propertyName = Name(expression);
            return typeof(TModel).GetProperty(propertyName);
        }
        public static PropertyInfo Get<TResult>(Expression<Func<TModel, TResult>> expression, BindingFlags bindingAttr)
        {
            var propertyName = Name(expression);
            return typeof(TModel).GetProperty(propertyName, bindingAttr);
        }

        public static string Name<TResult>(Expression<Func<TModel, TResult>> expression)
        {
            if (expression.Body.NodeType.Equals(ExpressionType.MemberAccess))
                return expression.ToString().Split('.').LastOrDefault();
            return null;
        }
        public static string Description<TResult>(Expression<Func<TModel, TResult>> expression)
        {
            return Get<DescriptionAttribute, TResult>(expression)?.Description;
        }


        /// <summary>
        /// 获取Model自定义DescriptionAttribute
        /// </summary>
        public static DescriptionAttribute DescriptionFor<TResult>(Expression<Func<TModel, TResult>> expression)
        {
            return Get<DescriptionAttribute, TResult>(expression);
        }
        public static DescriptionAttribute DescriptionFor(string propertyName)
        {
            return Get<DescriptionAttribute>(propertyName);
        }

        /// <summary>
        /// 获取Model自定义RequiredAttribute
        /// </summary>
        public static RequiredAttribute RequiredFor<TResult>(Expression<Func<TModel, TResult>> expression)
        {
            return Get<RequiredAttribute, TResult>(expression);
        }
        public static RequiredAttribute RequiredFor(string propertyName)
        {
            return Get<RequiredAttribute>(propertyName);
        }

        /// <summary>
        /// 获取Model自定义DisplayAttribute的Name
        /// </summary>
        public static DisplayAttribute DisplayNameFor<TResult>(Expression<Func<TModel, TResult>> expression)
        {
            return Get<DisplayAttribute, TResult>(expression);
        }
        public static DisplayAttribute DisplayNameFor(string propertyName)
        {
            return Get<DisplayAttribute>(propertyName);
        }


        public static StringLengthAttribute StringLengthFor<TResult>(Expression<Func<TModel, TResult>> expression)
        {
            return Get<StringLengthAttribute, TResult>(expression);
        }
        public static StringLengthAttribute StringLengthFor(string propertyName)
        {
            return Get<StringLengthAttribute>(propertyName);
        }

        public static FileSizeAttribute FileSizeFor<TResult>(Expression<Func<TModel, TResult>> expression)
        {
            return Get<FileSizeAttribute, TResult>(expression);
        }
        public static FileSizeAttribute FileSizeFor(string propertyName)
        {
            return Get<FileSizeAttribute>(propertyName);
        }


        #region 对象映射
        /// <summary>
        /// 对象映射并排除一些属性
        /// </summary>
        public static void MapTo<TResult, TField>(TModel model, TResult result, params Expression<Func<TModel, TField>>[] expressionExceptFields) where TResult : class
        {
            if (model == null || result == null) return;
            var mm = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var mr = typeof(TResult).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var ex = new List<string>();
            foreach (var x in expressionExceptFields)
            {
                var item = Name(x);
                if (item != null) ex.Add(item);
            }
            foreach (var m in mm)
            {
                if (mr.Any(p => p.Name.Equals(m.Name)) && !ex.Contains(m.Name))
                {
                    m.SetValue(result, m.GetValue(model));
                }
            }
        }
        /// <summary>
        /// 对象映射并排除一些属性
        /// </summary>
        public static TResult MapTo<TResult, TField>(TModel model, params Expression<Func<TModel, TField>>[] expressionExceptFields) where TResult : class
        {
            TResult result = (TResult)Activator.CreateInstance(typeof(TResult));
            MapTo(model, result, expressionExceptFields);
            return result;
        }
        #endregion

    }
}
