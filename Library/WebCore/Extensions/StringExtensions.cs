using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using ToolGood.Words;

namespace WebCore
{
    public static class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetUtf8MaxSize(this string value)
        {
            int ascii = 1; // We account for the end of the string.
            int nonAscii = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char v = value[i];
                if (v <= 0x7F)
                    ascii++;
                else if (v <= 0x7FF)
                    ascii += 2;
                else
                    nonAscii++;
            }

            // We can do 4 because unicode (string is unicode encoded) doesnt support 5 and 6 bytes values.
            int result = ascii + nonAscii * 4;
            Debug.Assert(result >= Encoding.UTF8.GetByteCount(value));

            return result;
        }


        /// <summary>
        /// 截断字符串长度
        /// </summary>
        public static string Substr(this string str, int maxlength)
        {
            if (str == null || maxlength <= 0 || str.Length <= maxlength) return str;

            return str.Substring(0, maxlength);
        }

        /// <summary>
        /// 判断一个数字是否在指定数组中
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool IsIn(this int value, params int[] values) => values.Contains(value);
        /// <summary>
        /// 判断一个数字是否在指定数组中
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool IsIn(this string value, params string[] values) => value == null ? false : values.Any(v => value.Equals(v, StringComparison.OrdinalIgnoreCase));
        /// <summary>
        /// 判断一个数字是否在指定数组中
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool IsIn(this Guid value, params Guid[] values) => values.Contains(value);

        /// <summary>
        /// 判断含有中文,中文字符集为[0x4E00,0x9FA5][0x3400,0x4db5]
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasChinese(this string str) => WordsHelper.HasChinese(str);
        /// <summary>
        /// 判断含有英语
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool HasEnglish(this string content) => WordsHelper.HasEnglish(content);
        /// <summary>
        /// 判断输入是否全为中文,中文字符集为[0x4E00,0x9FA5][0x3400,0x4db5]
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool IsAllChinese(this string content) => WordsHelper.IsAllChinese(content);
        /// <summary>
        /// 判断是否全部英语
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool IsAllEnglish(this string content) => WordsHelper.IsAllEnglish(content);
        /// <summary>
        /// 获取所有拼音,中文字符集为[0x3400,0x9FD5]，注：偏僻汉字很多未验证
        /// </summary>
        /// <param name="c"></param>
        /// <param name="tone"></param>
        /// <returns></returns>
        public static List<string> GetAllPinyin(this char c, bool tone = false) => WordsHelper.GetAllPinyin(c, tone);
        /// <summary>
        /// 获取首字母，中文字符集为[0x3400,0x9FD5],[0x20000-0x2B81D]，注：偏僻汉字很多未验证
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetFirstPinyin(this string text) => WordsHelper.GetFirstPinyin(text);
        /// <summary>
        /// 获取拼音全拼,支持多音,中文字符集为[0x4E00,0x9FD5],[0x20000-0x2B81D]，注：偏僻汉字很多未验证
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tone"></param>
        /// <returns></returns>
        public static string GetPinyin(this string text, bool tone = false) => WordsHelper.GetPinyin(text, tone);
        /// <summary>
        /// 获取拼音全拼,支持多音,中文字符集为[0x4E00,0x9FD5],[0x20000-0x2B81D]，注：偏僻汉字很多未验证
        /// </summary>
        /// <param name="text"></param>
        /// <param name="splitSpan"></param>
        /// <param name="tone"></param>
        /// <returns></returns>
        public static string GetPinyin(this string text, string splitSpan, bool tone = false) => WordsHelper.GetPinyin(text, splitSpan, tone);
        /// <summary>
        /// 获取姓名拼音,中文字符集为[0x3400,0x9FD5],[0x20000-0x2B81D]，注：偏僻汉字很多未验证
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tone"></param>
        /// <returns></returns>
        public static string GetPinyinForName(this string name, bool tone = false) => WordsHelper.GetPinyinForName(name, tone);
        /// <summary>
        /// 获取姓名拼音,中文字符集为[0x3400,0x9FD5],[0x20000-0x2B81D]，注：偏僻汉字很多未验证
        /// </summary>
        /// <param name="name"></param>
        /// <param name="splitSpan"></param>
        /// <param name="tone"></param>
        /// <returns></returns>
        public static string GetPinyinForName(this string name, string splitSpan, bool tone = false) => WordsHelper.GetPinyinForName(name, splitSpan, tone);
        /// <summary>
        /// 获取拼音全拼,支持多音,中文字符集为[0x4E00,0x9FD5],[0x20000-0x2B81D]，注：偏僻汉字很多未验证
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tone"></param>
        /// <returns></returns>
        public static string[] GetPinyinList(this string text, bool tone = false) => WordsHelper.GetPinyinList(text, tone);
        /// <summary>
        /// 获取姓名拼音,中文字符集为[0x3400,0x9FD5],[0x20000-0x2B81D]，注：偏僻汉字很多未验证
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tone"></param>
        /// <returns></returns>
        public static List<string> GetPinyinListForName(this string name, bool tone = false) => WordsHelper.GetPinyinListForName(name, tone);
        /// <summary>
        /// 转半角的函数
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToDBC(this string input) => WordsHelper.ToDBC(input);
        /// <summary>
        /// 半角转全角
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToSBC(this string input) => WordsHelper.ToSBC(input);
        /// <summary>
        /// 数字转中文大写
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string ToChineseRMB(this double x) => WordsHelper.ToChineseRMB(x);
        /// <summary>
        /// 中文转数字（支持中文大写）
        /// </summary>
        /// <param name="chineseString"></param>
        /// <returns></returns>
        public static decimal ToNumber(this string chineseString) => WordsHelper.ToNumber(chineseString);
        /// <summary>
        /// 转简体中文
        /// </summary>
        /// <param name="text"></param>
        /// <param name="srcType"></param>
        /// <returns></returns>
        public static string ToSimplifiedChinese(this string text, int srcType = 0) => WordsHelper.ToSimplifiedChinese(text, srcType);
        /// <summary>
        /// 转繁体中文
        /// </summary>
        /// <param name="text"></param>
        /// <param name="srcType"></param>
        /// <returns></returns>
        public static string ToTraditionalChinese(this string text, int srcType = 0) => WordsHelper.ToTraditionalChinese(text, srcType);


        /// <summary>
        /// Sql拼接 Id In()
        /// </summary>
        /// <param name="list"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string IdInList(this List<string> list, string id = "Id")
        {
            if (list == null || list.Count == 0) return "";
            if (list.Count == 1) return id + "='" + list[0].Replace("'", "") + "' ";
            if (list.Count <= 6) return "(" + string.Join(" OR ", list.Where(i => i != null).Select(i => id + "='" + i.Replace("'", "") + "'")) + ") ";
            return id + " in(" + string.Join(",", list.Where(i => i != null).Select(i => "'" + i.Replace("'", "") + "'")) + ") ";
        }
        /// <summary>
        /// Sql拼接 Id In()
        /// </summary>
        /// <param name="list"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string IdInList(this List<int> list, string id = "Id")
        {
            if (list == null || list.Count == 0) return "";
            if (list.Count == 1) return id + "=" + list[0] + " ";
            if (list.Count <= 6) return "(" + string.Join(" OR ", list.Select(i => id + "=" + i)) + ") ";
            return id + " in(" + string.Join(",", list.Select(i => "" + i)) + ") ";
        }

        /// <summary>
        /// 拼接 添加 Id
        /// </summary>
        /// <param name="list"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string IdAddToList(this string list, string id)
        {
            if (list == null || list.Trim() == "") return id;
            var s = list.Trim().Split(',').Distinct().Where(i => i.Trim() != "");
            if (s.Any(i => i.Equals(id, StringComparison.OrdinalIgnoreCase))) return string.Join(",", s);
            return string.Join(",", s.Concat(new string[] { id }));
        }
        /// <summary>
        /// 拼接 添加 Id
        /// </summary>
        /// <param name="list"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string IdAddToList(this string list, IEnumerable<string> id)
        {
            if (list == null || list.Trim() == "") return string.Join(",", id);
            var s = list.Trim().Split(',').Distinct().Where(i => i.Trim() != "");
            var l = new List<string>();
            foreach (string i in id) if (!s.Any(d => i.Equals(d, StringComparison.OrdinalIgnoreCase))) l.Add(i);
            return string.Join(",", s.Concat(l));
        }
        /// <summary>
        /// 拼接 移除 Id
        /// </summary>
        /// <param name="list"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string IdRemoveFromList(this string list, string id)
        {
            if (list == null || list.Trim() == "") return "";
            var s = list.Trim().Split(',').Distinct().Where(i => i.Trim() != "");
            var l = new List<string>();
            foreach (string i in s) if (!i.Equals(id, StringComparison.OrdinalIgnoreCase)) l.Add(i);
            return string.Join(",", l);
        }
        /// <summary>
        /// 拼接 移除 Id
        /// </summary>
        /// <param name="list"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string IdRemoveFromList(this string list, IEnumerable<string> id)
        {
            if (list == null || list.Trim() == "") return "";
            var s = list.Trim().Split(',').Distinct().Where(i => i.Trim() != "");
            var l = new List<string>();
            foreach (string i in s) if (!id.Any(d => i.Equals(d, StringComparison.OrdinalIgnoreCase))) l.Add(i);
            return string.Join(",", l);
        }

        /// <summary>
        /// 从尾部截取字符串
        /// </summary>
        /// <param name="s"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string Right(this string s, int i)
        {
            if (s == null || s.Length <= i) return s;
            return s.Substring(s.Length - i);
        }


        /// <summary>
        /// 验证邮箱
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsEmail(string s)
        {
            return Regex.IsMatch(s, @"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$");
        }

        /// <summary>
        /// 验证电话号码
        /// </summary>
        public static bool IsTelephone(this string s)
        {
            return !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^(\d{3,4}-)?\d{6,8}$");
        }

        /// <summary>
        /// 验证手机号码
        /// </summary>
        public static bool IsPhoneNumber(this string s)
        {
            return !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^1[0-9]{10}$");
        }

        /// <summary>
        /// 验证身份证号
        /// </summary>
        public static bool IsIdCard(this string s)
        {
            return !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"(^\d{18}$)|(^\d{15}$)");
        }

        /// <summary>
        /// 验证输入数字
        /// </summary>
        public static bool IsNumber(string s)
        {
            return !string.IsNullOrEmpty(s) && (int.TryParse(s, out _) || double.TryParse(s, out _));
        }

        /// <summary>
        /// 验证邮编
        /// </summary>
        public static bool IsPostalcode(string s)
        {
            return !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^\d{6}$");
        }

    }
}
