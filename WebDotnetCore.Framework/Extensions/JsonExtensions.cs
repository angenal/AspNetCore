using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace WebFramework.Extensions
{
    /// <summary>
    /// JSON .stringify .parse
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// JSON.stringify
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Stringify<T>(this T value)
        {
            return JsonConvert.SerializeObject(value);
        }
        /// <summary>
        /// JSON.parse
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T Parse<T>(this string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}
