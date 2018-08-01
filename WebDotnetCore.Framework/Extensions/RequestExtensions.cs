using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebFramework.Extensions
{
    public static class RequestExtensions
    {
        public static bool ContainsKeys(this IQueryCollection query, params string[] keys)
        {
            return !keys.Any(key => !query.ContainsKey(key));
        }
        public static bool ContainsKeysAny(this IQueryCollection query, params string[] keys)
        {
            return keys.Any(key => query.ContainsKey(key));
        }
        public static bool ContainsKeys(this IFormCollection form, params string[] keys)
        {
            return !keys.Any(key => !form.ContainsKey(key));
        }
        public static bool ContainsKeysAny(this IFormCollection form, params string[] keys)
        {
            return keys.Any(key => form.ContainsKey(key));
        }
    }
}
