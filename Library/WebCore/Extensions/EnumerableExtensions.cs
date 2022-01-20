using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebCore
{
    /// <summary>Provides extension methods for enumerations. </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// 自定义Distinct扩展方法 p => p.Id 或 p => new { p.Id, p.Name }
        /// </summary>
        /// <typeparam name="TSource">要去重的对象类</typeparam>
        /// <typeparam name="TKey">自定义去重的字段类型</typeparam>
        /// <param name="source">要去重的对象</param>
        /// <param name="keySelector">获取去重字段的委托</param>
        /// <returns></returns>
        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>Removes equal objects by specifing the comparing key.</summary>
        /// <typeparam name="TSource">The type of an item.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source enumerable.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns>The filtered enumerable.</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.GroupBy(keySelector).Select(g => g.First());
        }

        /// <summary>Removes equal objects by specifing the comparing key.</summary>
        /// <returns>The specifing element enumerable.</returns>
        public static IEnumerable<TElement> DistinctBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return source.GroupBy(keySelector, elementSelector).Select(g => g.First());
        }

        /// <summary>Whereifies the specified predicate funcs.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this">The this.</param>
        /// <param name="predicate">The predicate funcs.</param>
        /// <returns></returns>
        public static IEnumerable<T> Whereif<T>(this IEnumerable<T> @this, IEnumerable<Func<T, bool>> predicate)
        {
            using (var enumerator = @this.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (predicate.All(x => x.Invoke(current))) yield return current;
                }
            }
        }

        /// <summary>Provides ordering by two expressions. Use this method instaed of OrderBy(...).ThenBy(...) as it calls ThenBy only if necessary. </summary>
        public static IEnumerable<TSource> OrderBy<TSource, TKey1, TKey2>(this IEnumerable<TSource> source, Func<TSource, TKey1> orderBy1, Func<TSource, TKey2> orderBy2)
        {
            var result = new List<TSource>();
            var sorted = source.Select(s => new Tuple<TSource, TKey1>(s, orderBy1(s))).OrderBy(s => s.Item2).GroupBy(s => s.Item2);
            foreach (var s in sorted)
            {
                if (s.Count() > 1) result.AddRange(s.Select(p => p.Item1).OrderBy(orderBy2));
                else result.Add(s.First().Item1);
            }
            return result;
        }

        /// <summary>Returns true if the second list contains exactly the same items in the same order or is equal. </summary>
        /// <typeparam name="T">The item type. </typeparam>
        /// <param name="list1">The first list. </param>
        /// <param name="list2">The second list. </param>
        /// <returns></returns>
        public static bool IsCopyOf<T>(this IList<T> list1, IList<T> list2)
        {
            if ((list1 == null && list2 == null) || Equals(list1, list2))
                return true;

            if (list1 == null || list2 == null || list1.Count != list2.Count)
                return false;

            // Has same order
            for (int i = 0; i < list1.Count; i++) if (!Equals(list1[i], list2[i])) return false;

            // Has same elements
            if (list1.Any(a => !list2.Contains(a)) || list2.Any(a => !list1.Contains(a)))
                return false;

            return true;
        }

        /// <summary>Returns true if the second list contains exactly the same items or is equal. </summary>
        /// <typeparam name="T">The item type. </typeparam>
        /// <param name="list1">The first collection. </param>
        /// <param name="list2">The second collection. </param>
        /// <returns></returns>
        public static bool IsCopyOf<T>(this ICollection<T> list1, ICollection<T> list2)
        {
            if ((list1 == null && list2 == null) || Equals(list1, list2))
                return true;

            if (list1 == null || list2 == null || list1.Count != list2.Count)
                return false;

            // Has same elements
            if (list1.Any(a => !list2.Contains(a)) || list2.Any(a => !list1.Contains(a)))
                return false;

            return true;
        }

        /// <summary>Returns a shuffled list. </summary>
        /// <typeparam name="T">The item type. </typeparam>
        /// <param name="source">The list to shuffle. </param>
        /// <returns>The shuffled list. </returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            return source.Select(t => new KeyValuePair<int, T>(rand.Next(), t)).OrderBy(pair => pair.Key).Select(pair => pair.Value).ToArray();
        }

        /// <summary>Takes random items from the given list. </summary>
        /// <typeparam name="T">The item type. </typeparam>
        /// <param name="source">The list to take the items from. </param>
        /// <param name="amount">The amount of items to take. </param>
        /// <returns>The randomly taken items. </returns>
        public static IList<T> TakeRandom<T>(this IList<T> source, int amount = 0)
        {
            var list = new List<T>(source);
            var count = list.Count;
            var output = new List<T>();
            var rand = new Random((int)DateTime.Now.Ticks);
            if (amount < 1) amount = count;
            for (var i = 0; (0 < count) && (i < amount); i++)
            {
                var index = rand.Next(count);
                var item = list[index];
                output.Add(item);
                list.RemoveAt(index);
                count--;
            }
            return output;
        }

        /// <summary>Takes the minimal object from a list. </summary>
        /// <typeparam name="T">The item type. </typeparam>
        /// <typeparam name="U">The compared type. </typeparam>
        /// <param name="list">The list to search in. </param>
        /// <param name="selector">The selector of the object to compare. </param>
        /// <returns>The minimal object. </returns>
        public static T Min<T, U>(this IEnumerable<T> list, Func<T, U> selector) where T : class where U : IComparable
        {
            U resultValue = default;
            T result = null;
            foreach (var t in list)
            {
                var value = selector(t);
                if (result == null || value.CompareTo(resultValue) < 0)
                {
                    result = t;
                    resultValue = value;
                }
            }
            return result;
        }

        /// <summary>Takes the maximum object from a list. </summary>
        /// <typeparam name="T">The item type. </typeparam>
        /// <typeparam name="TProperty">The compared type. </typeparam>
        /// <param name="list">The list to search in. </param>
        /// <param name="selector">The selector of the object to compare. </param>
        /// <returns>The maximum object. </returns>
        public static T Max<T, TProperty>(this IEnumerable<T> list, Func<T, TProperty> selector) where T : class where TProperty : IComparable
        {
            TProperty resultValue = default;
            T result = null;
            foreach (var t in list)
            {
                var value = selector(t);
                if (result == null || value.CompareTo(resultValue) > 0)
                {
                    result = t;
                    resultValue = value;
                }
            }
            return result;
        }

        /// <summary>Gets a specified amount of items in the middle of a list. </summary>
        /// <typeparam name="T">The item type. </typeparam>
        /// <param name="list">The list. </param>
        /// <param name="count">The amount of items to retrieve. </param>
        /// <returns>The middle items. </returns>
        public static IList<T> Middle<T>(this IList<T> list, int count = 0)
        {
            if (list.Count == 0 || list.Count <= count)
                return list.ToList();

            var output = new List<T>();
            var startIndex = list.Count / 2 - count / 2;
            if (count < 1) count = list.Count - 2;
            for (var i = 0; i < count; i++) output.Add(list[startIndex + i]);
            return output;
        }

        /// <summary>Partitions an enumerable into blocks of a given size.</summary>
        /// <typeparam name="T">The item type. </typeparam>
        /// <param name="source">The source enumeration.</param>
        /// <param name="blockSize">Size of the block.</param>
        /// <returns>The partitions. </returns>
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int blockSize)
        {
            var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext()) yield return NextPartition(enumerator, blockSize);
        }

        /// <summary>Partitions an enumerable into blocks of a given size.</summary>
        private static IEnumerable<T> NextPartition<T>(IEnumerator<T> enumerator, int blockSize)
        {
            do { yield return enumerator.Current; }
            while (--blockSize > 0 && enumerator.MoveNext());
        }


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

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null || !enumerable.Any()) return;
            var s = enumerable.ToList();
            s.ForEach(action);
        }
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<IEnumerable<T>> action, int batchSize)
        {
            if (enumerable == null || !enumerable.Any()) return;
            var s = enumerable.ToList();
            if (s.Count == 0) return;
            if (s.Count <= batchSize || batchSize < 1)
            {
                action(s);
                return;
            }
            var batch = new List<T>(batchSize);
            var enumerator = s.GetEnumerator();
            while (enumerator.MoveNext())
            {
                batch.Add(enumerator.Current);
                if (batch.Count >= batchSize)
                {
                    action(batch);
                    batch.Clear();
                }
            }
            if (batch.Count > 0)
            {
                action(batch);
            }
        }
        public static async Task ForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> action)
        {
            if (enumerable == null || !enumerable.Any()) return;
            await Task.WhenAll(enumerable.Select(action));
        }
        public static async Task ForEachAsync<T>(this IEnumerable<T> enumerable, Func<IEnumerable<T>, Task> action, int batchSize)
        {
            if (enumerable == null) return;
            var s = enumerable.ToList();
            if (s.Count == 0) return;
            if (s.Count <= batchSize || batchSize < 1)
            {
                await action(s);
                return;
            }
            var batch = new List<T>(batchSize);
            var enumerator = s.GetEnumerator();
            while (enumerator.MoveNext())
            {
                batch.Add(enumerator.Current);
                if (batch.Count >= batchSize)
                {
                    await action(batch);
                    batch.Clear();
                }
            }
            if (batch.Count > 0)
            {
                await action(batch);
            }
        }
        public static async Task ForEachAsync<T>(this IAsyncEnumerable<T> enumerable, Func<IEnumerable<T>, Task> action, int batchSize = 2, CancellationToken cancellationToken = default)
        {
            if (enumerable == null || !await enumerable.AnyAsync()) return;
            if (batchSize < 2) batchSize = 2;
            var batch = new List<T>(batchSize);
            var enumerator = enumerable.WithCancellation(cancellationToken).ConfigureAwait(false).GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync())
            {
                batch.Add(enumerator.Current);
                if (batch.Count >= batchSize)
                {
                    await action(batch);
                    batch.Clear();
                }
            }
            if (batch.Count > 0)
            {
                await action(batch);
            }
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
        public static List<int> RandomArray(this int length, int min = 0) => ArrayGenerator.Generate(length, min);

        /// <summary>
        /// Random Array
        /// </summary>
        class ArrayGenerator
        {
            public static List<int> Generate(int length, int min = 0)
            {
                var rand = new Random((int)DateTime.Now.Ticks);
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
            static void Swap(ref List<int> nums, int i, int j)
            {
                var tmp = nums[i];
                nums[i] = nums[j];
                nums[j] = tmp;
            }
        }
    }
}
