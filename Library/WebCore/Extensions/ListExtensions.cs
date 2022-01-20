using System;
using System.Collections.Generic;

namespace WebCore
{
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
