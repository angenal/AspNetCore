using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
