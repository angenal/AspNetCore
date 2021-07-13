using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WebCore.Collections;

namespace WebCore
{
    /// <summary>Provides extension methods for dictionaries. </summary>
    public static class DictionaryExtensions
    {
        /// <summary>Recursively copies a dictionary. </summary>
        /// <param name="dictionary">The dictionary to copy. </param>
        /// <returns>The copied dictionary. </returns>
        public static Dictionary<string, object> DeepCopy(this Dictionary<string, object> dictionary)
        {
            var output = new Dictionary<string, object>();
            foreach (var p in dictionary)
            {
                if (p.Value is Dictionary<string, object>)
                    output[p.Key] = DeepCopy((Dictionary<string, object>)p.Value);
                else if (p.Value is List<Dictionary<string, object>>)
                {
                    var list = (List<Dictionary<string, object>>)p.Value;
                    output[p.Key] = list.Select(DeepCopy).ToList();
                }
                else
                    output[p.Key] = p.Value;
            }
            return output;
        }

        /// <summary>Recursively copies an observable dictionary. </summary>
        /// <param name="dictionary">The observable dictionary to copy. </param>
        /// <returns>The copied observable dictionary. </returns>
        public static ObservableDictionary<string, object> DeepCopy(this ObservableDictionary<string, object> dictionary)
        {
            var output = new ObservableDictionary<string, object>();
            foreach (var p in dictionary)
            {
                if (p.Value is ObservableDictionary<string, object>)
                    output[p.Key] = DeepCopy((ObservableDictionary<string, object>)p.Value);
                else if (p.Value is ObservableCollection<ObservableDictionary<string, object>>)
                {
                    var list = (ObservableCollection<ObservableDictionary<string, object>>)p.Value;
                    output[p.Key] = new ObservableCollection<ObservableDictionary<string, object>>(list.Select(DeepCopy));
                }
                else
                    output[p.Key] = p.Value;
            }
            return output;
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="orderBy"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static Dictionary<string, string> DefaultBy(this Dictionary<string, string> orderBy, string defaultValue) => orderBy == null || orderBy.Count == 0 ? ToDictionary(defaultValue) : orderBy;
        /// <summary>
        ///
        /// </summary>
        /// <param name="url"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDictionary(this string url, string defaultValue = null)
        {
            var dic = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(defaultValue))
                return ToDictionary(defaultValue);

            url = url.Trim();
            if (url.Contains("="))
            {
                if (url.IndexOf('?') != -1) url = url.Substring(1 + url.IndexOf('?'));
                var re = new Regex(@"(^|&)?(\w+)=(asc|desc)(&|$)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var mc = re.Matches(url);
                foreach (Match m in mc) dic.Add(m.Result("$2"), m.Result("$3"));
            }
            else if (url.Contains(" "))
            {
                foreach (var pair in url.Split(','))
                {
                    var s = pair.Split(' ');
                    if (s.Length == 2 && s[1].IsIn("asc", "desc")) dic.Add(s[0], s[1]);
                }
            }
            else if (url.StartsWith("{"))
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(url);
                foreach (var property in obj.GetType().GetProperties())
                {
                    var s = property.GetValue(obj)?.ToString();
                    if (s.IsIn("asc", "desc")) dic.Add(property.Name, s);
                }
            }
            return dic;
        }


        /// <summary>
        /// Sql拼接 Order By
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="tbl"></param>
        /// <returns></returns>
        public static string ToOrderBySql(this Dictionary<string, string> dic, string tbl = null)
        {
            if (dic == null) return null;
            if (tbl != null && !tbl.EndsWith(".")) tbl += ".";
            var s = new StringBuilder();
            foreach (var key in dic.Keys)
            {
                if (string.IsNullOrEmpty(key)) continue;
                s.AppendFormat("{0}{1} {2},", tbl, key, string.IsNullOrEmpty(dic[key]) ? "ASC" : dic[key]);
            }
            return s.Length == 0 ? null : s.ToString().TrimEnd(',');
        }

    }
}
