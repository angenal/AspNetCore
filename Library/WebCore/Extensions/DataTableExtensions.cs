using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace WebCore
{
    public static class DataTableExtensions
    {
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static List<T> ToEntities<T>(this DataTable table, params string[] excludes) where T : new()
        {
            if (table == null || Activator.CreateInstance(typeof(T)) == null) return null;
            var ps = typeof(T).GetProperties(ReflectionExtensions.PublicBindingAttr).Where(p => !excludes.Any(u => u.Equals(p.Name, StringComparison.OrdinalIgnoreCase))).ToArray();
            var entities = new List<T>();
            foreach (DataRow row in table.Rows)
            {
                T entity = (T)Activator.CreateInstance(typeof(T));
                foreach (var p in ps)
                {
                    if (!table.Columns.Contains(p.Name) || DBNull.Value == row[p.Name]) continue;
                    Type newType = p.PropertyType;
                    if (newType.IsGenericType && newType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                        newType = new NullableConverter(newType).UnderlyingType;
                    p.SetValue(entity, Convert.ChangeType(row[p.Name], newType), null);
                }
                entities.Add(entity);
            }
            return entities;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> entities, params string[] excludes) where T : new()
        {
            if (Activator.CreateInstance(typeof(T)) == null) return null;
            var ps = typeof(T).GetProperties(ReflectionExtensions.PublicBindingAttr).Where(p => !excludes.Any(u => u.Equals(p.Name, StringComparison.OrdinalIgnoreCase))).ToArray();
            var table = new DataTable();
            for (var i = 0; i < ps.Length; i++)
            {
                var p = ps[i];
                Type newType = p.PropertyType;
                if (newType.IsGenericType && newType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    newType = new NullableConverter(newType).UnderlyingType;
                table.Columns.Add(new DataColumn(p.Name, newType));
            }
            foreach (var entity in entities)
            {
                if (entity == null) continue;
                var values = new object[ps.Length];
                for (var i = 0; i < ps.Length; i++)
                    values[i] = ps[i].GetValue(entity);
                table.Rows.Add(values);
            }
            return table;
        }
    }
}
