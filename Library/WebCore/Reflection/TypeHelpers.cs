using System;
using System.Linq;
using System.Reflection;
using WebCore.Reflection;

namespace WebCore
{
    public static class TypeHelpers
    {
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
    }
}
