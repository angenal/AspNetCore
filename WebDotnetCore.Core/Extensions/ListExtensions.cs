using System;
using System.Collections.Generic;

namespace WebCore.Extensions
{
    public static class ListExtensions
    {
        public static T AddAndReturn<T>(this IList<T> list, T item)
        {
            list.Add(item);
            return item;
        }
    }
}
