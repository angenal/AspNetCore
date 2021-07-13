using System;
using System.Collections.Generic;

namespace WebCore
{
    public static class ListExtensions
    {
        public static T AddAndReturn<T>(this IList<T> list, T item)
        {
            list.Add(item);
            return item;
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
