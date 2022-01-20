using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using WebCore.Reflection;

namespace WebCore
{
    /// <summary>
    /// https://github.com/mgravell/fast-member
    /// </summary>
    public static class TypeHelpers
    {
        /// <summary>Fast access to fields/properties.</summary>
        public static void SetProperty<T>(this T obj, string propName, object value)
        {
            var accessor = TypeAccessor.Create(typeof(T));
            accessor[obj, propName] = value;
        }
        /// <summary>Fast access to fields/properties.</summary>
        public static void SetProperty<T>(this IEnumerable<T> objs, string propName, IEnumerable<object> values)
        {
            var accessor = TypeAccessor.Create(typeof(T));
            var arr1 = objs.ToArray();
            var arr2 = values.ToArray();
            for (int i = 0; i < arr1.Length; i++) accessor[arr1[i], propName] = arr2[i];
        }
        /// <summary>Fast access to fields/properties.</summary>
        public static object GetProperty(this object obj, string propName)
        {
            var wrapped = ObjectAccessor.Create(obj);
            return wrapped[propName];
        }
        /// <summary>Fast access to fields/properties.</summary>
        public static object GetProperty(this object obj, string propName, bool allowNonPublicAccessors)
        {
            var wrapped = ObjectAccessor.Create(obj, allowNonPublicAccessors);
            return wrapped[propName];
        }
        /// <summary>Fast access to fields/properties.</summary>
        public static IEnumerable<object> GetProperty<T>(this IEnumerable<T> objs, string propName)
        {
            var accessor = TypeAccessor.Create(typeof(T));
            var arr1 = objs.ToArray();
            var list = new List<object>();
            for (int i = 0; i < arr1.Length; i++) list.Add(accessor[arr1[i], propName]);
            return list;
        }

        public static int ForEach(this Type enumType, Action<(string name, int value, string description)> action)
        {
            if (enumType.BaseType != typeof(Enum)) return 0;
            var arr = Enum.GetNames(enumType);
            foreach (var name in arr)
            {
                var description = "";
                var value = Enum.Parse(enumType, name);
                var field = enumType.GetField(name);
                if (field != null)
                {
                    var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute), false) as DescriptionAttribute;
                    if (attr != null) description = attr.Description;
                }
                action((name, (int)value, description));
            }
            return arr.Length;
        }


        /// <summary>
        /// Represents an object whose members can be dynamically added and removed at runtime.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static System.Dynamic.ExpandoObject ToDynamic(this object source)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (source is System.Dynamic.ExpandoObject dlr) return dlr;
            dlr = new System.Dynamic.ExpandoObject();

            var type = source.GetType();
            var accessor = TypeAccessor.Create(type);
            if (accessor.GetMembersSupported)
            {
                var members = accessor.GetMembers();
                foreach (var member in members) dlr.SetProperty(member.Name, accessor[source, member.Name]);
            }
            else
            {
                var members = type.GetMembersInHierarchy();
                foreach (var member in members) dlr.SetProperty(member.Name, accessor[source, member.Name]);
            }

            return dlr;
        }


        /// <summary>
        /// 判断类型是否为集合类型
        /// </summary>
        public static bool IsEnumerable(this Type type) => IsDerivedFrom(type, typeof(IEnumerable<>));


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

        /// <summary>
        /// If type is a class, get its properties; if type is an interface, get its
        /// properties plus the properties of all the interfaces it inherits.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetTypeAndInterfaceProperties(this Type type, BindingFlags flags)
        {
            return !type.IsInterface ? type.GetProperties(flags) : (new[] { type }).Concat(type.GetInterfaces()).SelectMany(i => i.GetProperties(flags)).ToArray();
        }

        public static Type GetUnNullableType(this Type type)
        {
            if (!IsNullableType(type)) return type;
            var nullableConverter = new NullableConverter(type);
            return nullableConverter.UnderlyingType;
        }

        public static Type GetNonNummableType(this Type type)
        {
            return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
        }

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
            return IsGrouping(type.GetTypeInfo());
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
            if (!_commonTypeDictionary.TryGetValue(type, out object result))
            {
                return Activator.CreateInstance(type);
            }
            return result;
        }

        public static string GetTypeNameForSerialization(this Type t)
        {
            return RemoveAssemblyDetails(t.AssemblyQualifiedName);
        }

        private static string RemoveAssemblyDetails(string fullyQualifiedTypeName)
        {
            StringBuilder builder = new StringBuilder();

            // loop through the type name and filter out qualified assembly details from nested type names
            bool writingAssemblyName = false;
            bool skippingAssemblyDetails = false;
            for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
            {
                char current = fullyQualifiedTypeName[i];
                switch (current)
                {
                    case '[':
                        writingAssemblyName = false;
                        skippingAssemblyDetails = false;
                        builder.Append(current);
                        break;
                    case ']':
                        writingAssemblyName = false;
                        skippingAssemblyDetails = false;
                        builder.Append(current);
                        break;
                    case ',':
                        if (!writingAssemblyName)
                        {
                            writingAssemblyName = true;
                            builder.Append(current);
                        }
                        else
                        {
                            skippingAssemblyDetails = true;
                        }
                        break;
                    default:
                        if (!skippingAssemblyDetails)
                            builder.Append(current);
                        break;
                }
            }

            return builder.ToString();
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
