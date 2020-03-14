using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using WebCore.Attributes;

namespace WebFramework.Extensions
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
    }


    /// <summary>
    /// 类型<see cref="Type" />辅助方法扩展类
    /// </summary>
    public static class TypeExtensions
    {

        #region Nullable<>类型判断
        /// <summary>
        /// 判断类型是否为Nullable类型
        /// </summary>
        /// <param name="type"> 要处理的类型 </param>
        /// <returns> 是返回True，不是返回False </returns>
        public static bool IsNullableType(this Type type)
        {
            return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// 由类型的Nullable类型返回实际类型
        /// </summary>
        /// <param name="type"> 要处理的类型对象 </param>
        /// <returns> </returns>
        public static Type GetNonNummableType(this Type type)
        {
            if (IsNullableType(type))
                return type.GetGenericArguments()[0];
            return type;
        }

        /// <summary>
        /// 通过类型转换器获取Nullable类型的基础类型
        /// </summary>
        /// <param name="type"> 要处理的类型对象 </param>
        /// <returns> </returns>
        public static Type GetUnNullableType(this Type type)
        {
            if (IsNullableType(type))
            {
                var nullableConverter = new NullableConverter(type);
                return nullableConverter.UnderlyingType;
            }
            return type;
        }
        #endregion


        #region Attribute类型
        /// <summary>
        /// 获取成员元数据的Description特性描述信息
        /// </summary>
        /// <param name="member">成员元数据对象</param>
        /// <param name="inherit">是否搜索成员的继承链以查找描述特性</param>
        /// <returns>返回Description特性描述信息，如不存在则返回成员的名称</returns>
        public static string ToDescription(this MemberInfo member, bool inherit = false)
        {
            var desc = member.GetAttribute<DescriptionAttribute>(inherit);
            return desc == null ? member.Name : desc.Description;
        }

        /// <summary>
        /// 获取枚举项上的<see cref="DescriptionAttribute" />特性的文字描述
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToDescription(this Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            return member != null ? member.ToDescription() : value.ToString();
        }

        /// <summary>
        /// 检查指定指定类型成员中是否存在指定的Attribute特性
        /// </summary>
        /// <typeparam name="T">要检查的Attribute特性类型</typeparam>
        /// <param name="memberInfo">要检查的类型成员</param>
        /// <param name="inherit">是否从继承中查找</param>
        /// <returns>是否存在</returns>
        public static bool AttributeExists<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute
        {
            return memberInfo.GetCustomAttributes(typeof(T), inherit).Any(m => m as T != null);
        }

        /// <summary>
        /// 从类型成员获取指定Attribute特性
        /// </summary>
        /// <typeparam name="T">Attribute特性类型</typeparam>
        /// <param name="memberInfo">类型类型成员</param>
        /// <param name="inherit">是否从继承中查找</param>
        /// <returns>存在返回第一个，不存在返回null</returns>
        public static T GetAttribute<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute
        {
            var descripts = memberInfo.GetCustomAttributes(typeof(T), inherit);
            return descripts.FirstOrDefault() as T;
        }

        /// <summary>
        /// 从类型成员获取指定Attribute特性
        /// </summary>
        /// <typeparam name="T">Attribute特性类型</typeparam>
        /// <param name="memberInfo">类型类型成员</param>
        /// <param name="inherit">是否从继承中查找</param>
        /// <returns>返回所有指定Attribute特性的数组</returns>
        public static T[] GetAttributes<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute
        {
            return memberInfo.GetCustomAttributes(typeof(T), inherit).Cast<T>().ToArray();
        }
        #endregion


        /// <summary>
        /// 判断类型是否为集合类型
        /// </summary>
        public static bool IsEnumerable(this Type type) => IsDerivedFrom(type, typeof(IEnumerable<>));



        #region 类型是否继承类的扩展方法
        /// <summary>
        /// 判断 <paramref name="type"/> 指定的类型是否继承自 <paramref name="pattern"/> 指定的类型，或实现了 <paramref name="pattern"/> 指定的接口
        /// 支持未知类型参数的泛型，如typeof(List&lt;&gt;)
        /// </summary>
        /// <param name="type">需要测试的类型</param>
        /// <param name="pattern">要匹配的类型，如 typeof(int)，typeof(IEnumerable)，typeof(List&lt;&gt;)，typeof(List&lt;int&gt;)，typeof(IDictionary&lt;,&gt;)</param>
        /// <returns>如果 <paramref name="type"/> 指定的类型继承自 <paramref name="pattern"/> 指定的类型，或实现了 <paramref name="pattern"/> 指定的接口，则返回 true，否则返回 false</returns>
        public static bool IsDerivedFrom(this Type type, Type pattern)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            // 测试非泛型类型（如ArrayList）或确定类型参数的泛型类型（如List<int>，类型参数T已经确定为int）
            if (type.IsSubclassOf(pattern)) return true;
            // 测试非泛型接口（如IEnumerable）或确定类型参数的泛型接口（如IEnumerable<int>，类型参数T已经确定为int）
            if (pattern.IsAssignableFrom(type)) return true;
            // 测试泛型接口（如IEnumerable<>，IDictionary<,>，未知类型参数，留空）
            var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
            if (isTheRawGenericType) return true;
            // 测试泛型类型（如List<>，Dictionary<,>，未知类型参数，留空）
            while (type != null && type != typeof(object))
            {
                isTheRawGenericType = IsTheRawGenericType(type);
                if (isTheRawGenericType) return true;
                type = type.BaseType;
            }
            // 没有找到任何匹配的接口或类型
            return false;
            // 测试某个类型是否是指定的原始接口
            bool IsTheRawGenericType(Type test) => pattern == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
        }
        #endregion

        #region 枚举循环操作扩展方法
        public static int ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            var s = enumerable as T[] ?? enumerable.ToArray();
            foreach (var item in s) action(item);
            return s.Length;
        }
        public static async Task<int> ForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> action)
        {
            var s = enumerable as T[] ?? enumerable.ToArray();
            if (s.Any()) await Task.WhenAll(s.Select(action)); await Task.CompletedTask;
            return s.Length;
        }
        public static int ForEach(this Type enumType, Action<string, string, string> action)
        {
            if (enumType.BaseType != typeof(Enum)) return 0;
            var arr = Enum.GetValues(enumType);
            foreach (var name in arr)
            {
                string key = name.ToString(), description = "";
                var value = Enum.Parse(enumType, key);
                var fieldInfo = enumType.GetField(key);
                if (fieldInfo != null)
                {
                    var attr = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute), false) as DescriptionAttribute;
                    if (attr != null) description = attr.Description;
                }
                action(key, value.ToString(), description);
            }
            return arr.Length;
        }
        #endregion

        #region 对象映射
        /// <summary>
        /// 对象映射并排除一些属性
        /// </summary>
        public static void MapTo<TModel, TResult, TField>(this TModel model, TResult result, params Expression<Func<TModel, TField>>[] expressionExceptFields) where TModel : class where TResult : class
        {
            var mm = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var mr = typeof(TResult).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var ex = new List<string>();
            foreach (var x in expressionExceptFields)
            {
                var item = Attr<TModel>.Name(x);
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
        public static TResult MapTo<TModel, TResult, TField>(this TModel model, params Expression<Func<TModel, TField>>[] expressionExceptFields) where TModel : class where TResult : class
        {
            TResult result = (TResult)Activator.CreateInstance(typeof(TResult));
            MapTo(model, result, expressionExceptFields);
            return result;
        }
        #endregion

    }

}
