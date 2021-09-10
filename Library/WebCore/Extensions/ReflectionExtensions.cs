using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WebCore.Annotations;

namespace WebCore
{
    /// <summary>Provides additional reflection methods. </summary>
    [DebuggerStepThrough]
    public static class ReflectionExtensions
    {
        /// <summary>Gets the name of the type (without namespace or assembly version). </summary>
        /// <param name="type">The type. </param>
        /// <returns>The name of the type. </returns>
        public static string GetName(this Type type)
        {
            var fullName = type.FullName;
            var index = fullName.LastIndexOf('.');
            if (index != -1)
                return fullName.Substring(index + 1);

            return fullName;
        }

        /// <summary>Checks whether the given type inherits from a type with the given class name.</summary>
        /// <param name="type">The type.</param>
        /// <param name="typeName">THe class name of the type (not the full class name).</param>
        /// <returns>True when inheriting from the type name.</returns>
        public static bool InheritsFromTypeName(this Type type, string typeName)
        {
            var baseType = GetBaseType(type);
            while (baseType != null)
            {
                if (baseType.Name == typeName)
                    return true;

                baseType = GetBaseType(baseType);
            }
            return false;
        }


        /// <summary>Instantiates an object of a generic type. </summary>
        /// <param name="type">The type. </param>
        /// <param name="innerType">The first generic type. </param>
        /// <param name="args">The constructor parameters. </param>
        /// <returns>The instantiated object. </returns>
        public static object CreateGenericObject(this Type type, Type innerType, params object[] args)
        {
            var specificType = type.MakeGenericType(new[] { innerType });
            return Activator.CreateInstance(specificType, args);
        }

        /// <summary>Merges a given source object into a target object (no deep copy!). </summary>
        /// <param name="source">The source object. </param>
        /// <param name="target">The target object. </param>
        public static void Merge<T>(this T source, T target)
        {
            var targetType = target.GetType();

#if !LEGACY
            foreach (var p in source.GetType().GetRuntimeProperties())
            {
                var tp = targetType.GetRuntimeProperty(p.Name);
                if (tp != null && p.CanWrite)
                {
                    var value = p.GetValue(source, null);
                    tp.SetValue(target, value, null);
                }
            }
#else
            foreach (var p in source.GetType().GetProperties())
            {
                var tp = targetType.GetProperty(p.Name);
                if (tp != null && p.CanWrite)
                {
                    var value = p.GetValue(source, null);
                    tp.SetValue(target, value, null);
                }
            }
#endif
        }

#if !LEGACY

        public static IEnumerable<PropertyInfo> GetInheritedProperties(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            var list = typeInfo.DeclaredProperties.ToList();

            var subtype = typeInfo.BaseType;
            if (subtype != null)
                list.AddRange(subtype.GetRuntimeProperties());

            foreach (var i in typeInfo.ImplementedInterfaces)
                list.AddRange(i.GetRuntimeProperties());

            return list.ToArray();
        }

        public static PropertyInfo GetInheritedProperty(this Type type, string name)
        {
            var typeInfo = type.GetTypeInfo();

            var property = typeInfo.GetDeclaredProperty(name);
            if (property != null)
                return property;

            foreach (var i in typeInfo.ImplementedInterfaces)
            {
                property = i.GetRuntimeProperty(name);
                if (property != null)
                    return property;
            }

            var subtype = typeInfo.BaseType;
            if (subtype != null)
            {
                property = subtype.GetRuntimeProperty(name);
                if (property != null)
                    return property;
            }

            return null;
        }

        public static void Clone(this object source, object target)
        {
            var targetType = target.GetType();
            foreach (var p in source.GetType().GetInheritedProperties())
            {
                var tp = targetType.GetInheritedProperty(p.Name);
                if (tp != null && p.CanWrite)
                {
                    var value = p.GetValue(source, null);
                    tp.SetValue(target, value, null);
                }
            }
        }

#endif


        #region PropertyInfo
        public static bool IsStatic(this PropertyInfo property)
        {
            return (property.GetMethod ?? property.SetMethod).IsStatic;
        }

        public static bool IsCandidateProperty(this PropertyInfo propertyInfo, bool needsWrite = true, bool publicOnly = true)
        {
            return !propertyInfo.IsStatic() && propertyInfo.CanRead && (!needsWrite || propertyInfo.FindSetterProperty() != null) && propertyInfo.GetMethod != null && (!publicOnly || propertyInfo.GetMethod.IsPublic) && propertyInfo.GetIndexParameters().Length == 0;
        }

        public static PropertyInfo FindGetterProperty([NotNull] this PropertyInfo propertyInfo)
        {
            return propertyInfo.DeclaringType.GetPropertiesInHierarchy(propertyInfo.Name).FirstOrDefault((PropertyInfo p) => p.GetMethod != null);
        }

        public static PropertyInfo FindSetterProperty([NotNull] this PropertyInfo propertyInfo)
        {
            return propertyInfo.DeclaringType.GetPropertiesInHierarchy(propertyInfo.Name).FirstOrDefault((PropertyInfo p) => p.SetMethod != null);
        }
        #endregion

        #region MemberInfo

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
        /// 获取成员元数据的Description特性描述信息
        /// </summary>
        /// <param name="field">成员元数据对象</param>
        /// <param name="inherit">是否搜索成员的继承链以查找描述特性</param>
        /// <returns>返回Description特性描述信息，如不存在则返回成员的名称</returns>
        public static string ToDescription(this FieldInfo field, bool inherit = false)
        {
            var desc = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute), inherit) as DescriptionAttribute;
            return desc == null ? field.Name : desc.Description;
        }

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
        public static string ToDescription(this Enum value, bool nameInstead = true, bool inherit = false)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name == null) return null;
            FieldInfo field = type.GetField(name);
            var desc = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute), inherit) as DescriptionAttribute;
            if (desc == null && nameInstead == true) return name;
            return desc?.Description;
            //var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            //return member != null ? member.ToDescription() : value.ToString();
        }
        /// <summary>
        /// 获取枚举项上的<see cref="DescriptionAttribute" />特性的文字描述
        /// </summary>
        public static Dictionary<int, string> ToDescriptions(this Enum value, bool nameInstead = true, bool inherit = false)
        {
            return value.GetType().ToDescriptions(nameInstead, inherit);
        }
        /// <summary>
        /// 获取枚举类型的<see cref="DescriptionAttribute" />特性的文字描述
        /// </summary>
        public static Dictionary<int, string> ToDescriptions(this Type enumType, bool nameInstead = true, bool inherit = false)
        {
            var fields = enumType.GetFields();
            var values = Enum.GetValues(enumType);
            var names = Enum.GetNames(enumType);
            var dictionary = new Dictionary<int, string>();
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var field = fields.FirstOrDefault(t => t.Name == name);
                if (field == null) continue;
                var desc = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute), inherit) as DescriptionAttribute;
                if (desc == null && nameInstead == false) continue;
                dictionary.Add(Convert.ToInt32(values.GetValue(i)), desc == null ? name : desc.Description);
            }
            return dictionary;
        }
        /// <summary>
        /// 获取枚举类型的<see cref="DescriptionAttribute" />特性的文字描述
        /// </summary>
        public static Dictionary<string, string> ToDescriptionsFieldAsKey(this Type enumType, bool nameInstead = true, bool inherit = false)
        {
            var fields = enumType.GetFields();
            var names = Enum.GetNames(enumType);
            var dictionary = new Dictionary<string, string>();
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var field = fields.FirstOrDefault(t => t.Name == name);
                if (field == null) continue;
                var desc = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute), inherit) as DescriptionAttribute;
                if (desc == null && nameInstead == false) continue;
                dictionary.Add(name, desc == null ? name : desc.Description);
            }
            return dictionary;
        }
        /// <summary></summary>
        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            Type result;
            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            if ((result = (propertyInfo?.PropertyType)) == null)
            {
                FieldInfo fieldInfo = (FieldInfo)memberInfo;
                if (fieldInfo == null) return null;
                result = fieldInfo.FieldType;
            }
            return result;
        }
        /// <summary></summary>
        public static bool IsSameAs(this MemberInfo propertyInfo, MemberInfo otherPropertyInfo)
        {
            if (propertyInfo == null) return otherPropertyInfo == null;
            return !(otherPropertyInfo == null) && (Equals(propertyInfo, otherPropertyInfo) || (propertyInfo.Name == otherPropertyInfo.Name && (propertyInfo.DeclaringType == otherPropertyInfo.DeclaringType || propertyInfo.DeclaringType.GetTypeInfo().IsSubclassOf(otherPropertyInfo.DeclaringType) || otherPropertyInfo.DeclaringType.GetTypeInfo().IsSubclassOf(propertyInfo.DeclaringType) || propertyInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces.Contains(otherPropertyInfo.DeclaringType) || otherPropertyInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces.Contains(propertyInfo.DeclaringType))));
        }
        #endregion

        #region Type and Properties Info

        /// <summary>
        /// BindingFlags: Public Property {get;set}
        /// </summary>
        public static readonly BindingFlags PublicBindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty;

        /// <summary>
        /// 获取继承至某个类的所有公开类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypesOf<T>(this AppDomain domain, params string[] excludes) where T : class
        {
            foreach (Assembly assembly in domain.GetAssemblies())
                foreach (Type t in assembly.GetTypes().Where(t => t.IsPublic && t.IsClass && t.IsSubclassOf(typeof(T)) && !excludes.Any(u => u.Equals(t.Name, StringComparison.OrdinalIgnoreCase))))
                    yield return t;
        }

        /// <summary>
        /// 获取类型属性为一个字典对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static Dictionary<string, T> GetCustomAttributes<T>(this Type type, params string[] excludes) where T : Attribute
        {
            var fields = new Dictionary<string, T>();
            foreach (var p in type.GetProperties(PublicBindingAttr))
            {
                if (excludes.Any(u => u.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var attribute = Attribute.GetCustomAttribute(p, typeof(T));
                if (attribute == null)
                    continue;

                fields[p.Name] = attribute as T;
            }
            return fields;
        }

        /// <summary>
        /// 获取类型方法的特定属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static T GetCustomAttributes<T>(this Type type, string method, params string[] excludes) where T : Attribute
        {
            foreach (var p in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (excludes.Any(u => u.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var attribute = Attribute.GetCustomAttribute(p, typeof(T));
                if (attribute == null)
                    continue;

                if (p.Name.Equals(method, StringComparison.OrdinalIgnoreCase)) return attribute as T;
            }
            return type.GetCustomAttribute<T>();
        }

        /// <summary>
        /// 转换对象为一个哈希表对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static Hashtable ToHashtable<T>(this T obj, params string[] excludes) where T : class
        {
            if (obj == null) return null;
            var hashtable = new Hashtable();
            foreach (var p in obj.GetType().GetProperties(PublicBindingAttr))
            {
                if (excludes.Any(u => u.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;
                var v = p.GetValue(obj);
                hashtable.Add(p.Name, v);
            }
            return hashtable;
        }

        /// <summary>
        /// 转换对象列表为一组哈希表对象列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static IEnumerable<Hashtable> ToHashtables<T>(this IEnumerable<T> list, params string[] excludes) where T : class
        {
            if (list == null) return null;
            var hashtables = new List<Hashtable>();
            if (!list.Any()) return hashtables;
            var ps = list.First().GetType().GetProperties(PublicBindingAttr);
            foreach (var obj in list)
            {
                if (obj == null) continue;
                var hashtable = new Hashtable();
                foreach (var p in ps)
                {
                    if (excludes.Any(u => u.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                        continue;
                    var v = p.GetValue(obj);
                    hashtable.Add(p.Name, v);
                }
                hashtables.Add(hashtable);
            }
            return hashtables;
        }

        /// <summary>
        /// 转换对象为一个字典对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary<T>(this T obj, params string[] excludes) where T : class
        {
            if (obj == null) return null;
            var dictionary = new Dictionary<string, object>();
            foreach (var p in obj.GetType().GetProperties(PublicBindingAttr))
            {
                if (excludes.Any(u => u.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;
                var v = p.GetValue(obj);
                dictionary.Add(p.Name, v);
            }
            return dictionary;
        }

        /// <summary>
        /// 转换对象列表为一组字典对象列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static IEnumerable<Dictionary<string, object>> ToDictionaries<T>(this IEnumerable<T> list, params string[] excludes) where T : class
        {
            if (list == null) return null;
            var dictionaries = new List<Dictionary<string, object>>();
            if (!list.Any()) return dictionaries;
            var ps = list.First().GetType().GetProperties(PublicBindingAttr);
            foreach (var obj in list)
            {
                if (obj == null) continue;
                var dictionary = new Dictionary<string, object>();
                foreach (var p in ps)
                {
                    if (excludes.Any(u => u.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                        continue;
                    var v = p.GetValue(obj);
                    dictionary.Add(p.Name, v);
                }
                dictionaries.Add(dictionary);
            }
            return dictionaries;
        }

        /// <summary>
        /// 判断对象所有属性值都不为空
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static bool IsNotEmptyAllProperties(this object obj, params string[] excludes)
        {
            if (obj == null) return false;
            foreach (var p in obj.GetType().GetProperties(PublicBindingAttr))
            {
                if (excludes.Any(u => u.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;
                var v = p.GetValue(obj);
                if (v == null || v.ToString() == string.Empty) return false;
            }
            return true;
        }
        /// <summary>
        /// 判断对象所有属性值都为空
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static bool IsEmptyAllProperties(this object obj, params string[] excludes)
        {
            if (obj == null) return true;
            foreach (var p in obj.GetType().GetProperties(PublicBindingAttr))
            {
                if (excludes.Any(u => u.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;
                var v = p.GetValue(obj);
                if (v != null && v.ToString() != string.Empty) return false;
            }
            return true;
        }
        /// <summary>
        /// 获取对象属性列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
            return type.GetProperties(PublicBindingAttr);
        }
        /// <summary>
        /// 获取对象属性名称与值的列表
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static NameValueCollection GetProperties<T>(this T obj, PropertyInfo[] properties = null) where T : class
        {
            var values = new NameValueCollection();
            if (properties == null || properties.Length == 0)
            {
                if (obj == null) return values;
                properties = obj.GetType().GetProperties(PublicBindingAttr);
            }
            foreach (var p in properties)
            {
                var v = obj == null ? null : p.GetValue(obj);
                if (p.PropertyType.Namespace == "System")
                {
                    values.Set(p.Name, v?.ToString());
                    continue;
                }
                foreach (var p1 in p.PropertyType.GetProperties(PublicBindingAttr))
                {
                    values.Set(p.Name + "." + p1.Name, v == null ? null : p1.GetValue(v)?.ToString());
                }
            }
            return values;
        }
        /// <summary>
        /// 设置对象属性名称与值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="values"></param>
        /// <param name="properties"></param>
        public static void SetProperties<T>(this T obj, NameValueCollection values, PropertyInfo[] properties = null) where T : class
        {
            if (obj == null) return;
            var keys = values.AllKeys;
            if (keys.Length == 0) return;
            if (properties == null || properties.Length == 0)
                properties = obj.GetType().GetProperties(PublicBindingAttr);
            foreach (var p in properties)
            {
                string n = p.Name;
                if (p.PropertyType.Namespace == "System")
                {
                    string v = null;
                    foreach (string k in keys)
                    {
                        if (n.Equals(k, StringComparison.OrdinalIgnoreCase))
                        {
                            v = values[k];
                            continue;
                        }
                    }
                    if (v != null && v.Trim() != "") p.SetValue(obj, v);
                    continue;
                }
                var p0 = Activator.CreateInstance(p.PropertyType);
                foreach (var p1 in p.PropertyType.GetProperties(PublicBindingAttr))
                {
                    n = p.Name + "." + p1.Name;
                    string v = null;
                    foreach (string k in keys)
                    {
                        if (n.Equals(k, StringComparison.OrdinalIgnoreCase))
                        {
                            v = values[k];
                            continue;
                        }
                    }
                    if (v != null && v.Trim() != "") p1.SetValue(p0, v);
                }
                p.SetValue(obj, p0);
            }
        }

        #endregion

        #region LEGACY

#if !LEGACY
        /// <summary></summary>
        public static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return type.GetRuntimeProperties();
        }
        /// <summary></summary>
        public static PropertyInfo GetProperty(Type type, string name)
        {
            return type.GetRuntimeProperty(name);
        }
        /// <summary></summary>
        public static MethodInfo GetMethod(Type type, string name)
        {
            return type.GetRuntimeMethod(name, null);
        }
        /// <summary></summary>
        public static Type GetBaseType(Type type)
        {
            return type.GetTypeInfo().BaseType;
        }
        /// <summary></summary>
        public static IList<Type> GetGenericArguments(Type type)
        {
            return type.GenericTypeArguments;
        }
        /// <summary></summary>
        public static IEnumerable<Type> GetInterfaces(Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        }
#else
        /// <summary></summary>
        public static Type GetBaseType(Type type)
        {
            return type.BaseType;
        }
#endif
        #endregion

    }
}
