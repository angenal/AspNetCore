using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ToolGood.Words;

namespace WebCore
{
    public static class StringExtensions
    {
        /// <summary>
        /// Binary To Hex
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string BinaryToHex(this byte[] data) => data == null || data.Length == 0 ? string.Empty : Sodium.Utilities.BinaryToHex(data);

        /// <summary>
        /// Hex To Binary
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexToBinary(this string hex) => string.IsNullOrEmpty(hex) ? new byte[0] : Sodium.Utilities.HexToBinary(hex);

        /// <summary>
        /// Binary To Base64
        /// </summary>
        /// <param name="data"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        public static string BinaryToBase64(this byte[] data) => data == null || data.Length == 0 ? string.Empty : Sodium.Utilities.BinaryToBase64(data);

        /// <summary>
        /// Base64 To Binary
        /// </summary>
        /// <param name="base64"></param>
        /// <param name="ignoredChars"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        public static byte[] Base64ToBinary(this string base64, string ignoredChars) => string.IsNullOrEmpty(base64) ? new byte[0] : Sodium.Utilities.Base64ToBinary(base64, ignoredChars);

        /// <summary>
        /// Converts a string that has been encoded for transmission in a URL into a decoded string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlDecode(this string str) => string.IsNullOrEmpty(str) ? string.Empty : HttpUtility.UrlDecode(str);
        /// <summary>
        /// Converts a URL-encoded string into a decoded string, using the specified encoding object.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string UrlDecode(this string str, Encoding e) => string.IsNullOrEmpty(str) ? string.Empty : HttpUtility.UrlDecode(str, e);
        /// <summary>
        /// Converts a URL-encoded byte array into a decoded string using the specified decoding object.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string UrlDecode(this byte[] bytes, Encoding e) => bytes == null || bytes.Length == 0 ? string.Empty : HttpUtility.UrlDecode(bytes, e);

        /// <summary>
        /// Encodes a URL string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlEncode(this string str) => string.IsNullOrEmpty(str) ? string.Empty : HttpUtility.UrlEncode(str);
        /// <summary>
        /// Encodes a URL string using the specified encoding object.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string UrlEncode(this string str, Encoding e) => string.IsNullOrEmpty(str) ? string.Empty : HttpUtility.UrlEncode(str, e);
        /// <summary>
        /// Converts a byte array into an encoded URL string.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string UrlEncode(this byte[] bytes) => bytes == null || bytes.Length == 0 ? string.Empty : HttpUtility.UrlEncode(bytes);

        /// <summary>
        /// Compare a > b or a == b ...
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Compare(this byte[] a, byte[] b) => Sodium.Utilities.Compare(a, b);


        /// <summary>
        /// Replaces the name of each environment variable embedded in the specified string.
        /// for example "%WINDIR%"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string EnvironmentVariable(this string name) => Environment.ExpandEnvironmentVariables(name);

        /// <summary>Converts a string to a <see cref="Version"/> object. </summary>
        /// <param name="version">The version as string. </param>
        /// <returns>The version. </returns>
        public static Version FromString(this string version)
        {
            try
            {
                return !string.IsNullOrEmpty(version) ? new Version(version.Split('-')[0]) : new Version(0, 0, 0, 0);
            }
            catch (Exception)
            {
                return new Version(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Formats the specified MAC address.
        /// </summary>
        /// <param name="input">The MAC address to format.</param>
        /// <returns>The formatted MAC address.</returns>
        public static string FormatMacAddress(this string input)
        {
            // Check if this can be a hex formatted EUI-48 or EUI-64 identifier.
            if (input.Length != 12 && input.Length != 16)
            {
                return input;
            }

            // Chop up input in 2 character chunks.
            const int partSize = 2;
            var parts = Enumerable.Range(0, input.Length / partSize).Select(x => input.Substring(x * partSize, partSize));

            // Put the parts in the AA:BB:CC format.
            var result = string.Join(":", parts.ToArray());

            return result;
        }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string Concat(this string value, params string[] values)
        {
            var s = new List<string> { value };
            s.AddRange(values);
            return string.Concat(s);
        }
        /// <summary>
        /// ?????????????????????
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string Concat(this string value, IEnumerable<string> values)
        {
            var s = new List<string> { value };
            s.AddRange(values);
            return string.Concat(s);
        }
        /// <summary>
        /// ?????????????????????(???????????????)
        /// </summary>
        /// <param name="separator"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string Join(this string separator, IEnumerable<string> values) => string.Join(separator, values);
        /// <summary>
        /// ?????????????????????(???????????????)
        /// </summary>
        /// <param name="separator"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string Join(this string separator, params object[] values) => string.Join(separator, values);

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Format(this string format, params object[] args) => string.Format(format, args);

        /// <summary></summary>
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
        /// ?????????????????????
        /// </summary>
        public static string SplitComma(this string str, string comma = DefaultFormat.CommaChars) => string.IsNullOrEmpty(str) ? string.Empty : string.Join(",", str.Split(comma.ToCharArray()));

        /// <summary>
        /// ?????????????????????
        /// </summary>
        public static string Substr(this string str, int maxlength) => str == null || maxlength <= 0 || str.Length <= maxlength ? str : str.Substring(0, maxlength);

        /// <summary>
        /// ???????????????????????????????????????????????????????????????????????????????????????????????????
        /// </summary>
        /// <param name="str">???????????????</param>
        /// <param name="trim">???????????????</param>
        /// <returns></returns>
        public static string Trim(this string str, params string[] trim) => str.TrimStart(trim).TrimEnd(trim);

        /// <summary>
        /// ?????????????????????????????????????????????????????????????????????????????????????????????
        /// </summary>
        /// <param name="str">???????????????</param>
        /// <param name="starts">???????????????</param>
        /// <returns></returns>
        public static string TrimStart(this string str, params string[] starts)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (starts == null || starts.Length < 1 || string.IsNullOrEmpty(starts[0])) return str;

            for (var i = 0; i < starts.Length; i++)
            {
                if (str.StartsWith(starts[i], StringComparison.OrdinalIgnoreCase))
                {
                    str = str.Substring(starts[i].Length);
                    if (string.IsNullOrEmpty(str)) break;

                    // ????????????
                    i = -1;
                }
            }
            return str;
        }

        /// <summary>
        /// ?????????????????????????????????????????????????????????????????????????????????????????????
        /// </summary>
        /// <param name="str">???????????????</param>
        /// <param name="ends">???????????????</param>
        /// <returns></returns>
        public static string TrimEnd(this string str, params string[] ends)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (ends == null || ends.Length < 1 || string.IsNullOrEmpty(ends[0])) return str;

            for (var i = 0; i < ends.Length; i++)
            {
                if (str.EndsWith(ends[i], StringComparison.OrdinalIgnoreCase))
                {
                    str = str.Substring(0, str.Length - ends[i].Length);
                    if (string.IsNullOrEmpty(str)) break;

                    // ????????????
                    i = -1;
                }
            }
            return str;
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="i">????????????</param>
        /// <param name="digit">????????????</param>
        /// <param name="round">???????????????????????????</param>
        /// <returns></returns>
        public static string ToString(this double i, int digit, bool round = false)
        {
            var x = round ? digit - 1 : digit; var v = i.ToString($"f{digit + 1}");
            while (0 <= digit--) v = v.TrimEnd('0');
            v = v.TrimEnd('.'); var s = v.Split('.');
            if (s.Length == 2 && s[1].Length > x) v = string.Join(".", s[0], s[1].Substring(0, x));
            return v;
        }
        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="i">????????????</param>
        /// <param name="digit">????????????</param>
        /// <param name="round">???????????????????????????</param>
        /// <returns></returns>
        public static string ToString(this double? i, int digit, bool round = false)
        {
            return (i ?? 0).ToString(digit, round);
        }
        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="i">????????????</param>
        /// <param name="digit">????????????</param>
        /// <param name="round">???????????????????????????</param>
        /// <returns></returns>
        public static double Format(this double? i, int digit, bool round = false)
        {
            return double.Parse(i?.ToString(digit, round) ?? "0");
        }
        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="i">????????????</param>
        /// <param name="digit">????????????</param>
        /// <param name="round">???????????????????????????</param>
        /// <returns></returns>
        public static double Format(this double i, int digit, bool round = false)
        {
            return double.Parse(i.ToString(digit, round));
        }

        /// <summary>
        /// ???????????????????????????
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);
        /// <summary>
        /// ??????????????????????????????
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// ??????????????????????????????????????????
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool IsIn(this int value, params int[] values) => values.Contains(value);
        /// <summary>
        /// ??????????????????????????????????????????
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool IsIn(this string value, params string[] values) => !string.IsNullOrEmpty(value) && values.Any(v => value.Equals(v, StringComparison.OrdinalIgnoreCase));
        /// <summary>
        /// ??????????????????????????????????????????
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool IsIn(this Guid value, params Guid[] values) => values.Contains(value);

        /// <summary>
        /// ??????????????????,??????????????????[0x4E00,0x9FA5][0x3400,0x4db5]
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool HasChinese(this string content) => !string.IsNullOrEmpty(content) && WordsHelper.HasChinese(content);
        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool HasEnglish(this string content) => !string.IsNullOrEmpty(content) && WordsHelper.HasEnglish(content);
        /// <summary>
        /// ??????????????????????????????,??????????????????[0x4E00,0x9FA5][0x3400,0x4db5]
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool IsAllChinese(this string content) => !string.IsNullOrEmpty(content) && WordsHelper.IsAllChinese(content);
        /// <summary>
        /// ????????????????????????
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool IsAllEnglish(this string content) => !string.IsNullOrEmpty(content) && WordsHelper.IsAllEnglish(content);
        /// <summary>
        /// ??????????????????,??????????????????[0x3400,0x9FD5]????????????????????????????????????
        /// </summary>
        /// <param name="c"></param>
        /// <param name="tone"></param>
        /// <returns></returns>
        public static List<string> GetAllPinyin(this char c, bool tone = false) => WordsHelper.GetAllPinyin(c, tone);
        /// <summary>
        /// ????????????????????????????????????[0x3400,0x9FD5],[0x20000-0x2B81D]????????????????????????????????????
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetFirstPinyin(this string text) => string.IsNullOrEmpty(text) ? string.Empty : WordsHelper.GetFirstPinyin(text);
        /// <summary>
        /// ??????????????????,????????????,??????????????????[0x4E00,0x9FD5],[0x20000-0x2B81D]????????????????????????????????????
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tone"></param>
        /// <returns></returns>
        public static string GetPinyin(this string text, bool tone = false) => string.IsNullOrEmpty(text) ? string.Empty : WordsHelper.GetPinyin(text, tone);
        /// <summary>
        /// ??????????????????,????????????,??????????????????[0x4E00,0x9FD5],[0x20000-0x2B81D]????????????????????????????????????
        /// </summary>
        /// <param name="text"></param>
        /// <param name="splitSpan"></param>
        /// <param name="tone"></param>
        /// <returns></returns>
        public static string GetPinyin(this string text, string splitSpan, bool tone = false) => string.IsNullOrEmpty(text) ? string.Empty : WordsHelper.GetPinyin(text, splitSpan, tone);
        /// <summary>
        /// ??????????????????,??????????????????[0x3400,0x9FD5],[0x20000-0x2B81D]????????????????????????????????????
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tone"></param>
        /// <returns></returns>
        public static string GetPinyinForName(this string name, bool tone = false) => string.IsNullOrEmpty(name) ? string.Empty : WordsHelper.GetPinyinForName(name, tone);
        /// <summary>
        /// ??????????????????,??????????????????[0x3400,0x9FD5],[0x20000-0x2B81D]????????????????????????????????????
        /// </summary>
        /// <param name="name"></param>
        /// <param name="splitSpan"></param>
        /// <param name="tone"></param>
        /// <returns></returns>
        public static string GetPinyinForName(this string name, string splitSpan, bool tone = false) => string.IsNullOrEmpty(name) ? string.Empty : WordsHelper.GetPinyinForName(name, splitSpan, tone);
        /// <summary>
        /// ??????????????????,????????????,??????????????????[0x4E00,0x9FD5],[0x20000-0x2B81D]????????????????????????????????????
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tone"></param>
        /// <returns></returns>
        public static string[] GetPinyinList(this string text, bool tone = false) => string.IsNullOrEmpty(text) ? new string[0] : WordsHelper.GetPinyinList(text, tone);
        /// <summary>
        /// ??????????????????,??????????????????[0x3400,0x9FD5],[0x20000-0x2B81D]????????????????????????????????????
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tone"></param>
        /// <returns></returns>
        public static List<string> GetPinyinListForName(this string name, bool tone = false) => string.IsNullOrEmpty(name) ? new List<string>() : WordsHelper.GetPinyinListForName(name, tone);
        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToDBC(this string input) => string.IsNullOrEmpty(input) ? string.Empty : WordsHelper.ToDBC(input);
        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToSBC(this string input) => string.IsNullOrEmpty(input) ? string.Empty : WordsHelper.ToSBC(input);
        /// <summary>
        /// ?????????????????????
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string ToChineseRMB(this double x) => WordsHelper.ToChineseRMB(x);
        /// <summary>
        /// ???????????????????????????????????????
        /// </summary>
        /// <param name="chineseString"></param>
        /// <returns></returns>
        public static decimal ToNumber(this string chineseString) => WordsHelper.ToNumber(chineseString);
        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="text"></param>
        /// <param name="srcType"></param>
        /// <returns></returns>
        public static string ToSimplifiedChinese(this string text, int srcType = 0) => string.IsNullOrEmpty(text) ? string.Empty : WordsHelper.ToSimplifiedChinese(text, srcType);
        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="text"></param>
        /// <param name="srcType"></param>
        /// <returns></returns>
        public static string ToTraditionalChinese(this string text, int srcType = 0) => string.IsNullOrEmpty(text) ? string.Empty : WordsHelper.ToTraditionalChinese(text, srcType);

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToTitleCase(this string text) => string.IsNullOrEmpty(text) ? string.Empty : System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string value) => string.IsNullOrEmpty(value) ? string.Empty : string.Join(".", from part in value.Split(new char[] { '.' }, StringSplitOptions.None) select char.ToLowerInvariant(part[0]).ToString() + part.Substring(1));

        /// <summary>
        /// Sql?????? Id In()
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
        /// Sql?????? Id In()
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
        /// ?????? ?????? Id
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
        /// ?????? ?????? Id
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
        /// ?????? ?????? Id
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
        /// ?????? ?????? Id
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
        /// ????????????????????????
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
        /// ????????????
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsEmail(string s)
        {
            return Regex.IsMatch(s, @"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$");
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        public static bool IsTelephone(this string s)
        {
            return !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^(\d{3,4}-)?\d{6,8}$");
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        public static bool IsPhoneNumber(this string s)
        {
            return !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^1[0-9]{10}$");
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        public static bool IsIdCard(this string s)
        {
            return !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"(^\d{18}$)|(^\d{15}$)");
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        public static bool IsNumber(string s)
        {
            return !string.IsNullOrEmpty(s) && (int.TryParse(s, out _) || double.TryParse(s, out _));
        }

        /// <summary>
        /// ????????????
        /// </summary>
        public static bool IsPostalcode(string s)
        {
            return !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^\d{6}$");
        }

    }
}
