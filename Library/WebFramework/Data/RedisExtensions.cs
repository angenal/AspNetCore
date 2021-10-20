using EasyCaching.Core;
using System;
using System.Threading.Tasks;
using WebCore;

namespace WebFramework.Data
{
    public static class RedisExtensions
    {
        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="redis"></param>
        /// <param name="cacheKey"></param>
        /// <param name="camelCasePropertyNames">驼峰命名(首字母小写)</param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(this IRedisCachingProvider redis, string cacheKey, bool camelCasePropertyNames = true)
        {
            var s = await redis.StringGetAsync(cacheKey);
            return string.IsNullOrEmpty(s) ? default : s.ToObject<T>(camelCasePropertyNames);
        }
        /// <summary>
        /// 存储缓存对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="redis"></param>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <param name="expiration"></param>
        /// <param name="camelCasePropertyNames">驼峰命名(首字母小写)</param>
        /// <returns></returns>
        public static async Task SetAsync<T>(this IRedisCachingProvider redis, string cacheKey, T cacheValue, TimeSpan? expiration = null, bool camelCasePropertyNames = true)
        {
            if (cacheValue == null) return;
            var s = cacheValue.ToJson(camelCasePropertyNames);
            await redis.StringSetAsync(cacheKey, s, expiration);
        }
    }
}
