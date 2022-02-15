using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using WebCore.Reflection;

namespace WebCore
{
    /// <summary>
    /// Provides additional reflection methods.
    /// https://github.com/mgravell/fast-member
    /// </summary>
    [DebuggerStepThrough]
    public static class TypeHelpers
    {
        #region Objects

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
        /// <summary>Represents an object whose members can be dynamically added and removed at runtime.</summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static System.Dynamic.ExpandoObject ToDynamic(this object source, bool allowNonPublicAccessors = false)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (source is System.Dynamic.ExpandoObject dlr) return dlr;

            var type = source.GetType();
            dlr = new System.Dynamic.ExpandoObject();

            // TypeAccessor: Provides by-name member-access to objects of a given type
            var accessor = TypeAccessor.Create(type, allowNonPublicAccessors);
            if (accessor.GetMembersSupported)
            {
                // TypeAccessor: Query the members available for this type, Implementation provided by RuntimeTypeAccessor.
                var members = accessor.GetMembers();
                foreach (var member in members) dlr.SetProperty(member.Name, accessor[source, member.Name]);
            }
            else
            {
                // Retrieves a collection that represents all the fields and properties defined on a specified type at run time.
                var members = type.GetMembersInHierarchy();
                foreach (var member in members) dlr.SetProperty(member.Name, accessor[source, member.Name]);
            }

            return dlr;
        }

        #endregion


        #region Assembly and AppDomain

        /// <summary>
        /// 获取继承至某个类或接口的 Assemblies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentDomain"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAssembliesIs<T>(this AppDomain currentDomain, params string[] excludes) where T : class
        {
            var assemblies = Assemblies.All.Any() ? Assemblies.All : currentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies.Where(a => !a.IsDynamic && a.ExportedTypes.Any(t => t.IsClass && typeof(T).IsAssignableFrom(t) && !excludes.Any(u => u.Equals(t.Name, StringComparison.OrdinalIgnoreCase)))))
                yield return assembly;
        }

        /// <summary>
        /// 获取继承至某个父类的 Assemblies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentDomain"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAssembliesIsSubclassOf<T>(this AppDomain currentDomain, params string[] excludes) where T : class
        {
            var assemblies = Assemblies.All.Any() ? Assemblies.All : currentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies.Where(a => !a.IsDynamic && a.ExportedTypes.Any(t => t.IsClass && t.IsSubclassOf(typeof(T)) && !excludes.Any(u => u.Equals(t.Name, StringComparison.OrdinalIgnoreCase)))))
                yield return assembly;
        }

        /// <summary>
        /// 获取继承至某个类或接口的 Assemblies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assemblies"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAssembliesIs<T>(this IEnumerable<Assembly> assemblies, params string[] excludes) where T : class
        {
            foreach (Assembly assembly in assemblies.Where(a => !a.IsDynamic && a.ExportedTypes.Any(t => t.IsClass && typeof(T).IsAssignableFrom(t) && !excludes.Any(u => u.Equals(t.Name, StringComparison.OrdinalIgnoreCase)))))
                yield return assembly;
        }

        /// <summary>
        /// 获取继承至某个父类的 Assemblies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assemblies">System.Runtime.Loader.AssemblyLoadContext.Default.Assemblies</param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAssembliesIsSubclassOf<T>(this IEnumerable<Assembly> assemblies, params string[] excludes) where T : class
        {
            foreach (Assembly assembly in assemblies.Where(a => !a.IsDynamic && a.ExportedTypes.Any(t => t.IsClass && t.IsSubclassOf(typeof(T)) && !excludes.Any(u => u.Equals(t.Name, StringComparison.OrdinalIgnoreCase)))))
                yield return assembly;
        }

        /// <summary>
        /// Gets all the assemblies referenced specified type with a custom attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly">Assembly.GetEntryAssembly()</param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetReferencedAssembliesHasAttribute<T>(this Assembly assembly) where T : Attribute
        {
            return assembly.GetReferencedAssemblies().Select(Assembly.Load).HasAttribute<T>();
        }


        /// <summary>
        /// 获取继承至某个类的所有公开类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assemblies">System.Runtime.Loader.AssemblyLoadContext.Default.Assemblies</param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypesIs<T>(this IEnumerable<Assembly> assemblies, params string[] excludes) where T : class
        {
            foreach (Assembly assembly in assemblies.Where(a => !a.IsDynamic && a.ExportedTypes.Any(t => t.IsClass && typeof(T).IsAssignableFrom(t))))
                foreach (Type t in assembly.GetTypes().Where(t => t.IsPublic && t.IsClass && typeof(T).IsAssignableFrom(t) && !excludes.Any(u => u.Equals(t.Name, StringComparison.OrdinalIgnoreCase))))
                    yield return t;
        }

        /// <summary>
        /// 获取继承至某个类的所有公开类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assemblies">System.Runtime.Loader.AssemblyLoadContext.Default.Assemblies</param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypesOf<T>(this IEnumerable<Assembly> assemblies, params string[] excludes) where T : class
        {
            foreach (Assembly assembly in assemblies.Where(a => !a.IsDynamic && a.ExportedTypes.Any(t => t.IsClass && t.IsSubclassOf(typeof(T)))))
                foreach (Type t in assembly.GetTypes().Where(t => t.IsPublic && t.IsClass && t.IsSubclassOf(typeof(T)) && !excludes.Any(u => u.Equals(t.Name, StringComparison.OrdinalIgnoreCase))))
                    yield return t;
        }


        /// <summary>
        /// Filtering the assemblies referenced specified type with a custom attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assemblies">System.Runtime.Loader.AssemblyLoadContext.Default.Assemblies</param>
        /// <returns></returns>
        public static IEnumerable<Assembly> HasAttribute<T>(this IEnumerable<Assembly> assemblies) where T : Attribute
        {
            return assemblies.Where(a => !a.IsDynamic && a.ExportedTypes.Any(t => t.GetCustomAttribute<T>() != null));
        }

        public static IEnumerable<TypeInfo> GetConstructibleTypes(this Assembly assembly)
        {
            return from t in assembly.GetLoadableDefinedTypes() where !t.IsAbstract && !t.IsGenericTypeDefinition select t;
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
                result = (from t in ex.Types where t != null select t).Select(new Func<Type, TypeInfo>(IntrospectionExtensions.GetTypeInfo));
            }
            return result;
        }

        #endregion


        #region Enum Type

        /// <summary>
        /// Loop to read the value and description of enumeration type
        /// </summary>
        /// <param name="enumType">enumeration type</param>
        /// <param name="action">loop to read the value and description</param>
        /// <returns></returns>
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
        /// 获取枚举项上的<see cref="Attribute" />特性
        /// </summary>
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            MemberInfo memberInfo = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            return memberInfo == null ? default : (T)memberInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();
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

        #endregion


        #region General Type

        /// <summary>The factories</summary>
        private static readonly ConcurrentDictionary<ConstructorInfo, Func<object[], object>> Factories = new ConcurrentDictionary<ConstructorInfo, Func<object[], object>>();


        /// <summary>
        /// BindingFlags: Public Property {get;set}
        /// </summary>
        public static readonly BindingFlags PublicBindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty;


        /// <summary>Creates the instance.</summary>
        /// <typeparam name="TBase">The type of the base.</typeparam>
        /// <param name="subtypeofTBase">The subtypeof t base.</param>
        /// <param name="ctorArgs">The ctor arguments.</param>
        /// <returns></returns>
        public static TBase CreateInstance<TBase>(this Type subtypeofTBase, params object[] args)
        {
            EnsureIsAssignable<TBase>(subtypeofTBase);

            return Instantiate<TBase>(subtypeofTBase, args ?? new object[0]);
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

        /// <summary>Instantiates the specified ctor arguments.</summary>
        /// <param name="ctor">The ctor.</param>
        /// <param name="ctorArgs">The ctor arguments.</param>
        /// <returns></returns>
        public static object Instantiate(this ConstructorInfo ctor, object[] ctorArgs)
        {
            Func<object[], object> factory;

            factory = Factories.GetOrAdd(ctor, BuildFactory);

            return factory.Invoke(ctorArgs);
        }

        /// <summary>Determines whether [is].</summary>
        /// <typeparam name="TType">The type of the type.</typeparam>
        /// <param name="type">The type.</param>
        /// <returns>
        ///     <c>true</c> if [is] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Is<TType>(this Type type)
        {
            return typeof(TType).IsAssignableFrom(type);
        }


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

        /// <summary>Retrieves a collection that represents all the fields and properties defined on a specified type at run time.</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> GetMembersInHierarchy(this Type type, bool allowNonPublicAccessors = false)
        {
            do
            {
                if (allowNonPublicAccessors)
                {
                    foreach (PropertyInfo propertyInfo in from pi in type.GetRuntimeProperties() where !(pi.GetMethod ?? pi.SetMethod).IsStatic select pi)
                    {
                        yield return propertyInfo;
                    }
                    foreach (FieldInfo fieldInfo in from f in type.GetRuntimeFields() where !f.IsStatic select f)
                    {
                        yield return fieldInfo;
                    }
                }
                else
                {
                    foreach (PropertyInfo propertyInfo in from pi in type.GetRuntimeProperties() where pi.GetMethod != null && !pi.GetMethod.IsStatic && pi.GetMethod.IsPublic select pi)
                    {
                        yield return propertyInfo;
                    }
                    foreach (FieldInfo fieldInfo in from f in type.GetRuntimeFields() where !f.IsStatic && f.IsPublic select f)
                    {
                        yield return fieldInfo;
                    }
                }
                type = type.BaseType;
            }
            while (type != null);
            yield break;
        }

        public static IEnumerable<MemberInfo> GetMembersInHierarchy(this Type type, string name, bool allowNonPublicAccessors = false)
        {
            return from m in type.GetMembersInHierarchy(allowNonPublicAccessors) where m.Name == name select m;
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

#endif

        /// <summary>Builds the factory.</summary>
        /// <param name="ctor">The ctor.</param>
        /// <returns></returns>
        private static Func<object[], object> BuildFactory(ConstructorInfo ctor)
        {
            var parameterInfos = ctor.GetParameters();
            var parameterExpressions = new Expression[parameterInfos.Length];
            var argument = Expression.Parameter(typeof(object[]), "parameters");
            for (var i = 0; i < parameterExpressions.Length; i++)
                parameterExpressions[i] = Expression.Convert(
                    Expression.ArrayIndex(argument, Expression.Constant(i, typeof(int))),
                    parameterInfos[i].ParameterType.IsByRef
                        ? parameterInfos[i].ParameterType.GetElementType()
                        : parameterInfos[i].ParameterType);

            return Expression.Lambda<Func<object[], object>>(
                Expression.New(ctor, parameterExpressions),
                argument).Compile();
        }

        /// <summary>Instantiates the specified subtypeof t base.</summary>
        /// <typeparam name="TBase">The type of the base.</typeparam>
        /// <param name="subtypeofTBase">The subtypeof t base.</param>
        /// <param name="ctorArgs">The ctor arguments.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.Exception"></exception>
        private static TBase Instantiate<TBase>(Type subtypeofTBase, object[] ctorArgs)
        {
            ctorArgs = ctorArgs ?? new object[0];
            var types = ctorArgs.ConvertAll(a => a == null ? typeof(object) : a.GetType());
            var constructor = subtypeofTBase.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, types, null);
            if (constructor != null) return (TBase)Instantiate(constructor, ctorArgs);

            try
            {
                return (TBase)Activator.CreateInstance(subtypeofTBase, ctorArgs);
            }
            catch (MissingMethodException ex)
            {
                string message;
                if (ctorArgs.Length == 0)
                {
                    message =
                        $"Type {subtypeofTBase.FullName} does not have a public default constructor and could not be instantiated.";
                }
                else
                {
                    var messageBuilder = new StringBuilder();
                    messageBuilder.AppendLine(
                        $"Type {subtypeofTBase.FullName} does not have a public constructor matching arguments of the following types:");
                    foreach (var type in ctorArgs.Select(o => o.GetType())) messageBuilder.AppendLine(type.FullName);

                    message = messageBuilder.ToString();
                }

                throw new ArgumentException(message, ex);
            }
            catch (Exception ex)
            {
                var message = $"Could not instantiate {subtypeofTBase.FullName}.";
                throw new Exception(message, ex);
            }
        }

        /// <summary>Ensures the is assignable.</summary>
        /// <typeparam name="TBase">The type of the base.</typeparam>
        /// <param name="subtypeofTBase">The subtypeof t base.</param>
        /// <exception cref="System.InvalidCastException"></exception>
        private static void EnsureIsAssignable<TBase>(Type subtypeofTBase)
        {
            if (subtypeofTBase.Is<TBase>()) return;

            var message = typeof(TBase).IsInterface
                ? $"Type {subtypeofTBase.FullName} does not implement the interface {typeof(TBase).FullName}."
                : $"Type {subtypeofTBase.FullName} does not inherit from {typeof(TBase).FullName}.";

            throw new InvalidCastException(message);
        }


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

        #endregion


        #region PropertyInfo
        public static bool IsStatic(this PropertyInfo property)
        {
            return (property.GetMethod ?? property.SetMethod).IsStatic;
        }

        public static bool IsCandidateProperty(this PropertyInfo propertyInfo, bool needsWrite = true, bool publicOnly = true)
        {
            return !propertyInfo.IsStatic() && propertyInfo.CanRead && (!needsWrite || propertyInfo.FindSetterProperty() != null) && propertyInfo.GetMethod != null && (!publicOnly || propertyInfo.GetMethod.IsPublic) && propertyInfo.GetIndexParameters().Length == 0;
        }

        public static PropertyInfo FindGetterProperty(this PropertyInfo propertyInfo)
        {
            return propertyInfo.DeclaringType.GetPropertiesInHierarchy(propertyInfo.Name).FirstOrDefault((PropertyInfo p) => p.GetMethod != null);
        }

        public static PropertyInfo FindSetterProperty(this PropertyInfo propertyInfo)
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
    }
}
