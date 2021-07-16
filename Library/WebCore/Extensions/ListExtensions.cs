using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace WebCore
{
    public static class ListExtensions
    {
        public static bool ContainsKeys(this IQueryCollection query, params string[] keys)
        {
            return !keys.Any(key => !query.ContainsKey(key));
        }
        public static bool ContainsKeysAny(this IQueryCollection query, params string[] keys)
        {
            return keys.Any(key => query.ContainsKey(key));
        }
        public static bool ContainsKeys(this IFormCollection form, params string[] keys)
        {
            return !keys.Any(key => !form.ContainsKey(key));
        }
        public static bool ContainsKeysAny(this IFormCollection form, params string[] keys)
        {
            return keys.Any(key => form.ContainsKey(key));
        }

        public static T AddAndReturn<T>(this IList<T> list, T item)
        {
            list.Add(item);
            return item;
        }

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

        /// <summary>
        /// 生成随机整数数组
        /// </summary>
        /// <param name="length"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static List<int> RandomArray(this int length, int min = 0) => new ArrayGenerator().Generate(length, min);

    }

    /// <summary>
    /// Random Array
    /// </summary>
    public class ArrayGenerator
    {
        private readonly Random rand = new Random(Guid.NewGuid().GetHashCode());
        public List<int> Generate(int length, int min = 0)
        {
            var list = new List<int>();
            for (int i = 0; i < length; i++)
                list.Add(i + min);

            for (int i = 0; i < list.Count; i++)
            {
                int next = rand.Next(0, i + 1);
                Swap(ref list, i, next);
            }

            return list;
        }
        internal void Swap(ref List<int> nums, int i, int j)
        {
            var tmp = nums[i];
            nums[i] = nums[j];
            nums[j] = tmp;
        }
    }

}
