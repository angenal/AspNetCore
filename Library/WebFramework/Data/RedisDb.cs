using StackExchange.Redis;
using System;
using System.Collections.Generic;
using WebCore;
using WebCore.Models.DTO;

namespace WebFramework.Data
{
    /// <summary>
    /// Redis List
    /// </summary>
    public static class RedisList
    {
        /// <summary></summary>
        public static void Add<T>(string key, T value)
        {
            if (0 < Redis.ListRightPush(new RedisKey(key), new RedisValue(value.ToJson())))
                Redis.StringIncrement(key + IncrementKeyTail);
        }

        /// <summary></summary>
        public static void Add<T>(string key, T[] value)
        {
            var l = value.Length;
            if (l == 0) return;

            var values = new List<RedisValue>();
            for (var i = 0; i < l; i++) values.Add(new RedisValue(value[i].ToJson()));

            if (0 < Redis.ListRightPush(new RedisKey(key), values.ToArray()))
                Redis.StringIncrement(key + IncrementKeyTail, l);
        }


        /// <summary></summary>
        public static IEnumerable<T> GetLastestResult<T>(string key, int size = 20)
        {
            var startingFrom = 0;
            if (size > 0)
            {
                var s = Redis.StringGet(key + IncrementKeyTail);
                if (s.HasValue && int.TryParse(s.ToString(), out var count) && count > size)
                    startingFrom = count - size;
            }
            var list = Redis.ListRange(new RedisKey(key), startingFrom);

            var rows = new List<T>();
            foreach (var item in list) rows.Add(item.ToString().ToObject<T>());

            return rows;
        }

        /// <summary></summary>
        public static PageOutputDto<T> GetPagingResult<T>(string key, int pageIndex = 1, int pageSize = 20, bool orderDesc = true)
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 1;

            var result = new PageOutputDto<T>(pageIndex, pageSize);

            var s = Redis.StringGet(key + IncrementKeyTail);
            if (!s.HasValue || !int.TryParse(s.ToString(), out var count) || count == 0) return result;

            result.PageNumber = (int)Math.Ceiling((double)count / pageSize);
            var startingFrom = orderDesc ? count - pageIndex * pageSize : (pageIndex - 1) * pageSize;
            if (startingFrom >= count) return result;

            var list = Redis.ListRange(new RedisKey(key), startingFrom, startingFrom + pageSize - 1);
            if (list.Length == 0) return result;

            var rows = new List<T>();
            foreach (var item in list) rows.Add(item.ToString().ToObject<T>());

            return new PageOutputDto<T>(rows, pageIndex, pageSize, result.PageNumber);
        }

        /// <summary></summary>
        public static void Remove(string key)
        {
            var k = new RedisKey(key);
            if (!Redis.KeyExists(k)) return;
            if (Redis.KeyDelete(k)) Redis.KeyDelete(new RedisKey(key + IncrementKeyTail));
        }

        /// <summary>
        /// Get redis db instance
        /// </summary>
        private static IDatabase Redis => RedisManager.GetDatabase();

        private const string IncrementKeyTail = "C";
    }
}
