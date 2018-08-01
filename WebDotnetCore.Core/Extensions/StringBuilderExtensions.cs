using System;
using System.Collections.Generic;
using System.Text;

namespace WebCore.Extensions
{
    public static class StringBuilderExtensions
    {
		public static StringBuilder AppendJoin(this StringBuilder stringBuilder, IEnumerable<string> values, string separator = ", ")
        {
            return stringBuilder.AppendJoin(values, delegate (StringBuilder sb, string value)
            {
                sb.Append(value);
            }, separator);
        }

        public static StringBuilder AppendJoin(this StringBuilder stringBuilder, string separator, params string[] values)
        {
            return stringBuilder.AppendJoin(values, delegate (StringBuilder sb, string value)
            {
                sb.Append(value);
            }, separator);
        }

        public static StringBuilder AppendJoin<T>(this StringBuilder stringBuilder, IEnumerable<T> values, Action<StringBuilder, T> joinAction, string separator = ", ")
        {
            bool flag = false;
            foreach (T arg in values)
            {
                joinAction(stringBuilder, arg);
                stringBuilder.Append(separator);
                flag = true;
            }
            if (flag)
            {
                stringBuilder.Length -= separator.Length;
            }
            return stringBuilder;
        }

        public static StringBuilder AppendJoin<T, TParam>(this StringBuilder stringBuilder, IEnumerable<T> values, TParam param, Action<StringBuilder, T, TParam> joinAction, string separator = ", ")
        {
            bool flag = false;
            foreach (T arg in values)
            {
                joinAction(stringBuilder, arg, param);
                stringBuilder.Append(separator);
                flag = true;
            }
            if (flag)
            {
                stringBuilder.Length -= separator.Length;
            }
            return stringBuilder;
        }

        public static StringBuilder AppendJoin<T, TParam1, TParam2>(this StringBuilder stringBuilder, IEnumerable<T> values, TParam1 param1, TParam2 param2, Action<StringBuilder, T, TParam1, TParam2> joinAction, string separator = ", ")
        {
            bool flag = false;
            foreach (T arg in values)
            {
                joinAction(stringBuilder, arg, param1, param2);
                stringBuilder.Append(separator);
                flag = true;
            }
            if (flag)
            {
                stringBuilder.Length -= separator.Length;
            }
            return stringBuilder;
        }
    }
}
