using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Reflection;

namespace WebCore
{
    /// <summary>Provides additional object common methods. </summary>
    [DebuggerStepThrough]
    public static class ObjectExtensions
    {
        /// <summary>
        /// BindingFlags: Public Property {get;set}
        /// </summary>
        public static readonly BindingFlags PublicBindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty;

        /// <summary>Casts the specified this.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this">The this.</param>
        /// <returns></returns>
        public static T As<T>(this object @this)
        {
            Check.NotNull(@this, nameof(@this));

            return (T)@this;
        }


        /// <summary>
        /// 转换时间
        /// </summary>
        public static readonly JsonConverter[] JsonConverters = new JsonConverter[] { new Newtonsoft.Json.Converters.IsoDateTimeConverter { DateTimeFormat = DefaultFormat.DateTime } };

        /// <summary>
        /// 驼峰命名(首字母小写)
        /// </summary>
        public static Func<JsonSerializerSettings> CamelCaseJsonSerializerSettings => () => new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        /// <summary>首字母大写</summary>
        public static Func<JsonSerializerSettings> DefaultJsonSerializerSettings => () => new JsonSerializerSettings() { ContractResolver = new DefaultContractResolver() };

        /// <summary>
        /// Javascript: JSON.stringify
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="camelCasePropertyNames">驼峰命名(首字母小写)</param>
        /// <returns></returns>
        public static string Stringify<T>(this T obj, bool camelCasePropertyNames = true) => ToJson(obj, camelCasePropertyNames);
        /// <summary>
        /// Javascript: JSON.parse
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="camelCasePropertyNames">驼峰命名(首字母小写)</param>
        /// <returns></returns>
        public static T Parse<T>(this string s, bool camelCasePropertyNames = true) => ToObject<T>(s, camelCasePropertyNames);

        /// <summary>
        /// 对象转换JSON字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="camelCasePropertyNames">驼峰命名(首字母小写)</param>
        /// <returns></returns>
        public static string ToJson<T>(this T obj, bool camelCasePropertyNames = true)
        {
            JsonConvert.DefaultSettings = camelCasePropertyNames ? CamelCaseJsonSerializerSettings : DefaultJsonSerializerSettings;
            return JsonConvert.SerializeObject(obj, JsonConverters);
        }

        /// <summary>
        /// JSON字符串转换对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="camelCasePropertyNames">驼峰命名(首字母小写)</param>
        /// <returns></returns>
        public static T ToObject<T>(this string s, bool camelCasePropertyNames = true)
        {
            JsonConvert.DefaultSettings = camelCasePropertyNames ? CamelCaseJsonSerializerSettings : DefaultJsonSerializerSettings;
            return JsonConvert.DeserializeObject<T>(s);
        }
    }
}
