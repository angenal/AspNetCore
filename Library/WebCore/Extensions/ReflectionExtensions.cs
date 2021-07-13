using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using WebCore.Annotations;

namespace WebCore
{
    /// <summary>Provides additional reflection methods. </summary>
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
            var baseType = ReflectionUtilities.GetBaseType(type);
            while (baseType != null)
            {
                if (baseType.Name == typeName)
                    return true;

                baseType = ReflectionUtilities.GetBaseType(baseType);
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
        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            Type result;
            if ((result = ((propertyInfo != null) ? propertyInfo.PropertyType : null)) == null)
            {
                FieldInfo fieldInfo = (FieldInfo)memberInfo;
                if (fieldInfo == null)
                {
                    return null;
                }
                result = fieldInfo.FieldType;
            }
            return result;
        }

        public static bool IsSameAs(this MemberInfo propertyInfo, MemberInfo otherPropertyInfo)
        {
            if (propertyInfo == null)
            {
                return otherPropertyInfo == null;
            }
            return !(otherPropertyInfo == null) && (object.Equals(propertyInfo, otherPropertyInfo) || (propertyInfo.Name == otherPropertyInfo.Name && (propertyInfo.DeclaringType == otherPropertyInfo.DeclaringType || propertyInfo.DeclaringType.GetTypeInfo().IsSubclassOf(otherPropertyInfo.DeclaringType) || otherPropertyInfo.DeclaringType.GetTypeInfo().IsSubclassOf(propertyInfo.DeclaringType) || propertyInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces.Contains(otherPropertyInfo.DeclaringType) || otherPropertyInfo.DeclaringType.GetTypeInfo().ImplementedInterfaces.Contains(propertyInfo.DeclaringType))));
        }
        #endregion


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

    }

    [DebuggerStepThrough]
    public static class SharedTypeExtensions
    {
        public static Type UnwrapNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        public static bool IsNullableType(this Type type)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            return !typeInfo.IsValueType || (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static bool IsValidEntityType(this Type type)
        {
            return type.GetTypeInfo().IsClass;
        }

        public static Type MakeNullable(this Type type, bool nullable = true)
        {
            if (type.IsNullableType() == nullable)
            {
                return type;
            }
            if (!nullable)
            {
                return type.UnwrapNullableType();
            }
            return typeof(Nullable<>).MakeGenericType(new Type[]
            {
                type
            });
        }

        public static bool IsInteger(this Type type)
        {
            type = type.UnwrapNullableType();
            return type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) || type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte) || type == typeof(char);
        }

        public static PropertyInfo GetAnyProperty(this Type type, string name)
        {
            List<PropertyInfo> list = (from p in type.GetRuntimeProperties()
                                       where p.Name == name
                                       select p).ToList();
            if (list.Count > 1)
            {
                throw new AmbiguousMatchException();
            }
            return list.SingleOrDefault();
        }

        public static bool IsInstantiable(this Type type)
        {
            return IsInstantiable(type.GetTypeInfo());
        }

        private static bool IsInstantiable(TypeInfo type)
        {
            return !type.IsAbstract && !type.IsInterface && (!type.IsGenericType || !type.IsGenericTypeDefinition);
        }

        public static bool IsGrouping(this Type type)
        {
            return SharedTypeExtensions.IsGrouping(type.GetTypeInfo());
        }

        private static bool IsGrouping(TypeInfo type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IGrouping<,>) || type.GetGenericTypeDefinition() == typeof(IAsyncGrouping<,>));
        }

        public static Type UnwrapEnumType(this Type type)
        {
            bool flag = type.IsNullableType();
            Type type2 = flag ? type.UnwrapNullableType() : type;
            if (!type2.GetTypeInfo().IsEnum)
            {
                return type;
            }
            Type underlyingType = Enum.GetUnderlyingType(type2);
            if (!flag)
            {
                return underlyingType;
            }
            return underlyingType.MakeNullable(true);
        }

        public static Type GetSequenceType(this Type type)
        {
            Type type2 = type.TryGetSequenceType();
            if (type2 == null)
            {
                throw new ArgumentException();
            }
            return type2;
        }

        public static Type TryGetSequenceType(this Type type)
        {
            return type.TryGetElementType(typeof(IEnumerable<>)) ?? type.TryGetElementType(typeof(IAsyncEnumerable<>));
        }

        public static Type TryGetElementType(this Type type, Type interfaceOrBaseType)
        {
            if (type.GetTypeInfo().IsGenericTypeDefinition)
            {
                return null;
            }
            IEnumerable<Type> genericTypeImplementations = type.GetGenericTypeImplementations(interfaceOrBaseType);
            Type type2 = null;
            foreach (Type type3 in genericTypeImplementations)
            {
                if (!(type2 == null))
                {
                    type2 = null;
                    break;
                }
                type2 = type3;
            }
            if (type2 == null)
            {
                return null;
            }
            return type2.GetTypeInfo().GenericTypeArguments.FirstOrDefault<Type>();
        }

        public static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericTypeDefinition)
            {
                IEnumerable<Type> enumerable = interfaceOrBaseType.GetTypeInfo().IsInterface ? typeInfo.ImplementedInterfaces : type.GetBaseTypes();
                foreach (Type type2 in enumerable)
                {
                    if (type2.GetTypeInfo().IsGenericType && type2.GetGenericTypeDefinition() == interfaceOrBaseType)
                    {
                        yield return type2;
                    }
                }
                if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == interfaceOrBaseType)
                {
                    yield return type;
                }
            }
            yield break;
        }

        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            type = type.GetTypeInfo().BaseType;
            while (type != null)
            {
                yield return type;
                type = type.GetTypeInfo().BaseType;
            }
            yield break;
        }

        public static IEnumerable<Type> GetTypesInHierarchy(this Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.GetTypeInfo().BaseType;
            }
            yield break;
        }

        public static ConstructorInfo GetDeclaredConstructor(this Type type, Type[] types)
        {
            types = (types ?? Array.Empty<Type>());
            return type.GetTypeInfo().DeclaredConstructors.SingleOrDefault(delegate (ConstructorInfo c)
            {
                if (!c.IsStatic)
                {
                    return (from p in c.GetParameters()
                            select p.ParameterType).SequenceEqual(types);
                }
                return false;
            });
        }

        public static IEnumerable<PropertyInfo> GetPropertiesInHierarchy(this Type type, string name)
        {
            do
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                PropertyInfo declaredProperty = typeInfo.GetDeclaredProperty(name);
                if (declaredProperty != null && !(declaredProperty.GetMethod ?? declaredProperty.SetMethod).IsStatic)
                {
                    yield return declaredProperty;
                }
                type = typeInfo.BaseType;
                typeInfo = null;
            }
            while (type != null);
            yield break;
        }

        public static IEnumerable<MemberInfo> GetMembersInHierarchy(this Type type)
        {
            do
            {
                foreach (PropertyInfo propertyInfo in from pi in type.GetRuntimeProperties()
                                                      where !(pi.GetMethod ?? pi.SetMethod).IsStatic
                                                      select pi)
                {
                    yield return propertyInfo;
                }
                foreach (FieldInfo fieldInfo in from f in type.GetRuntimeFields()
                                                where !f.IsStatic
                                                select f)
                {
                    yield return fieldInfo;
                }
                type = type.BaseType;
            }
            while (type != null);
            yield break;
        }

        public static IEnumerable<MemberInfo> GetMembersInHierarchy(this Type type, string name)
        {
            return from m in type.GetMembersInHierarchy()
                   where m.Name == name
                   select m;
        }

        public static object GetDefaultValue(this Type type)
        {
            if (!type.GetTypeInfo().IsValueType)
            {
                return null;
            }
            object result;
            if (!SharedTypeExtensions._commonTypeDictionary.TryGetValue(type, out result))
            {
                return Activator.CreateInstance(type);
            }
            return result;
        }

        public static IEnumerable<TypeInfo> GetConstructibleTypes(this Assembly assembly)
        {
            return from t in assembly.GetLoadableDefinedTypes()
                   where !t.IsAbstract && !t.IsGenericTypeDefinition
                   select t;
        }

        public static IEnumerable<TypeInfo> GetLoadableDefinedTypes(this Assembly assembly)
        {
            IEnumerable<TypeInfo> result;
            try
            {
                result = assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException ex)
            {
                result = (from t in ex.Types
                          where t != null
                          select t).Select(new Func<Type, TypeInfo>(IntrospectionExtensions.GetTypeInfo));
            }
            return result;
        }

        private static readonly Dictionary<Type, object> _commonTypeDictionary = new Dictionary<Type, object>
        {
            {
                typeof(int),
                0
            },
            {
                typeof(Guid),
                default(Guid)
            },
            {
                typeof(DateTime),
                default(DateTime)
            },
            {
                typeof(DateTimeOffset),
                default(DateTimeOffset)
            },
            {
                typeof(long),
                0L
            },
            {
                typeof(bool),
                0
            },
            {
                typeof(double),
                0.0
            },
            {
                typeof(short),
                0
            },
            {
                typeof(float),
                0f
            },
            {
                typeof(byte),
                0
            },
            {
                typeof(char),
                0
            },
            {
                typeof(uint),
                0
            },
            {
                typeof(ushort),
                0
            },
            {
                typeof(ulong),
                0L
            },
            {
                typeof(sbyte),
                0
            }
        };
    }
}
