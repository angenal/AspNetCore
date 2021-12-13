using Microsoft.ClearScript;
using NATS.Services.Util;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace NATS.Services.V8Script
{
    public static class ExtensionObject
    {
        public static string ToJson<T>(this T obj) => JsonConvert.SerializeObject(obj, NewtonsoftJson.Converters);

        public static T ToObject<T>(this string s) => JsonConvert.DeserializeObject<T>(s);

        public static List<int> ToListInt(this ScriptObject obj)
        {
            var p = new List<int>();
            foreach (string attr in obj.GetDynamicMemberNames())
            {
                var v = obj.GetProperty(attr);
                if (v is int i) p.Add(i);
            }
            return p;
        }

        public static List<string> ToListString(this ScriptObject obj)
        {
            var p = new List<string>();
            foreach (string attr in obj.GetDynamicMemberNames())
            {
                var v = obj.GetProperty(attr);
                if (v is string i && !string.IsNullOrWhiteSpace(i)) p.Add(i);
            }
            return p;
        }

        public static Dictionary<string, object> ToDictionary(this ScriptObject obj)
        {
            var p = new Dictionary<string, object>();
            if (obj == null) return p;
            foreach (var key in obj.PropertyNames) p.Add(key, obj.GetProperty(key));
            return p;
        }

        public static string ToStr(this Dictionary<string, object> p)
        {
            var s = new StringBuilder();
            if (p != null) foreach (var k in p.Keys) { s.Append(k); s.Append(p[k]); }
            return s.ToString();
        }
    }
}
