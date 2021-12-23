using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
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

        public static TResult[] ConvertAll<T, TResult>(this T[] items, Converter<T, TResult> transformation)
        {
            return Array.ConvertAll(items, transformation);
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


        /// <summary>Checks whether or not collection is null or empty. Assumes colleciton can be safely enumerated multiple times.</summary>
        /// <param name="this">The this.</param>
        /// <returns>
        ///     <c>true</c> if [is null or empty] [the specified this]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty(this IEnumerable @this)
        {
            if (@this != null) return !@this.GetEnumerator().MoveNext();

            return true;
        }

        /// <summary>Finds the specified predicate.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static T Find<T>(this T[] items, Predicate<T> predicate)
        {
            return Array.Find(items, predicate);
        }

        /// <summary>Finds all.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static T[] FindAll<T>(this T[] items, Predicate<T> predicate)
        {
            return Array.FindAll(items, predicate);
        }


        /// <summary>
        ///     Concatenate the given byte arrays.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The byte array.</param>
        /// <param name="arrays">The byte arrays.</param>
        /// <returns>The concatenated byte arrays.</returns>
        /// <exception cref="OverflowException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static T[] ConcatArrays<T>(this T[] array, params T[][] arrays)
        {
            checked
            {
                var result = new T[array.Length + arrays.Sum(arr => arr.Length)];
                var offset = 0;

                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;

                foreach (var arr in arrays)
                {
                    Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                    offset += arr.Length;
                }

                return result;
            }
        }

        /// <summary>
        ///     Concatenate two byte arrays.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr1">The first byte array.</param>
        /// <param name="arr2">The second byte array.</param>
        /// <returns>The concatenated byte arrays.</returns>
        /// <exception cref="OverflowException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static T[] ConcatArrays<T>(this T[] arr1, T[] arr2)
        {
            checked
            {
                var result = new T[arr1.Length + arr2.Length];
                Buffer.BlockCopy(arr1, 0, result, 0, arr1.Length);
                Buffer.BlockCopy(arr2, 0, result, arr1.Length, arr2.Length);

                return result;
            }
        }

        /// <summary>
        ///     Extract a part of a byte array from another byte array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr">A byte array.</param>
        /// <param name="start">Position to start extraction.</param>
        /// <param name="length">The length of the extraction started at start.</param>
        /// <returns>A part with the given length of the byte array.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static T[] SubArray<T>(this T[] arr, int start, int length)
        {
            var result = new T[length];
            Buffer.BlockCopy(arr, start, result, 0, length);

            return result;
        }

        /// <summary>
        ///     Extract a part of a byte array from another byte array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr">A byte array.</param>
        /// <param name="start">Position to start extraction.</param>
        /// <returns>A part of the given byte array.</returns>
        /// <exception cref="OverflowException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static T[] SubArray<T>(this T[] arr, int start)
        {
            return SubArray(arr, start, arr.Length - start);
        }

        /// <summary>
        ///     Constant-time comparison of two byte arrays.
        /// </summary>
        /// <param name="a">The first byte array.</param>
        /// <param name="b">The second byte array.</param>
        /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
        /// <exception cref="OverflowException"></exception>
        public static bool ConstantTimeEquals(this byte[] a, byte[] b)
        {
            var diff = (uint)a.Length ^ (uint)b.Length;
            for (var i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
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
        private readonly Random rand = new Random((int)DateTime.Now.Ticks);
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
