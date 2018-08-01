using System;
using System.Collections.Generic;
using System.Reflection;

namespace WebCore
{
    public class ReflectionUtilities
    {
#if !LEGACY
        public static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return type.GetRuntimeProperties();
        }

        public static PropertyInfo GetProperty(Type type, string name)
        {
            return type.GetRuntimeProperty(name);
        }

        public static MethodInfo GetMethod(Type type, string name)
        {
            return type.GetRuntimeMethod(name, null);
        }

        public static Type GetBaseType(Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public static IList<Type> GetGenericArguments(Type type)
        {
            return type.GenericTypeArguments;
        }

        public static IEnumerable<Type> GetInterfaces(Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        }
#else
        public static Type GetBaseType(Type type)
        {
            return type.BaseType;
        }
#endif
    }
}