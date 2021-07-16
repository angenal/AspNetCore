namespace WebCore
{
    /// <summary>Provides additional object common methods. </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// 转换时间
        /// </summary>
        public static readonly Newtonsoft.Json.JsonConverter[] JsonConverters = new Newtonsoft.Json.JsonConverter[] { new Newtonsoft.Json.Converters.IsoDateTimeConverter { DateTimeFormat = DefaultFormat.DateTimeFormats } };

        /// <summary>
        /// Javascript: JSON.stringify
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Stringify<T>(this T obj) => ToJson(obj);
        /// <summary>
        /// Javascript: JSON.parse
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public static T Parse<T>(this string s) => ToObject<T>(s);

        /// <summary>
        /// 对象转换JSON字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson<T>(this T obj) => Newtonsoft.Json.JsonConvert.SerializeObject(obj, JsonConverters);

        /// <summary>
        /// JSON字符串转换对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public static T ToObject<T>(this string s) => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(s);

    }
}
