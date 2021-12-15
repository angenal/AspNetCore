using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using NATS.Services.Util;
using Newtonsoft.Json;
using WebInterface;

namespace NATS.Services.V8Script
{
    /// <summary>
    /// 缓存功能
    /// </summary>
    public sealed class JS_Cache
    {
        /// <summary>
        /// 缓存方式从参数获取
        /// </summary>
        /// <param name="index"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private CacheType ArgTypes(int index, params object[] args) => args.Length > index && int.TryParse(args[index].ToString(), out int t) ? t == 1 ? CacheType.Redis : t == 2 ? CacheType.All : CacheType.Memory : CacheType.Memory;

        readonly V8ScriptEngine Engine;

        readonly string Prefix;
        readonly string Subject;

        public string prefix => Prefix;
        public string subject => Subject;

        public JS_Cache(Config.RedisConfig config, V8ScriptEngine engine, string prefix, string subject)
        {
            WebCore.Cache.Memory.Instance = new WebCore.Cache.Memory();
            WebCore.Cache.Redis.Instance = new WebCore.Cache.Redis(config.Addr, config.Password, config.Db);
            Engine = engine;
            Prefix = prefix;
            Subject = subject;
        }

        /// <summary>
        /// $cache.get("key",0) // Memory:0, Cache{ Memory = 0, Redis, Default }
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object get(params object[] args)
        {
            var length = args.Length;
            if (length == 0)
                return null;

            string k = args[0].ToString();
            if (string.IsNullOrEmpty(k)) return null;

            string code = Get(ArgTypes(1), k);
            if (string.IsNullOrEmpty(code)) return null;

            return Engine.Evaluate(JS.SecurityCode(code));
        }

        /// <summary>
        /// Get Cache
        /// </summary>
        /// <param name="types"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static string Get(CacheType types, string k)
        {
            string v = null;
            if (string.IsNullOrEmpty(k)) return v;
            switch (types)
            {
                case CacheType.Memory:
                    v = WebCore.Cache.Memory.Instance.Get<string>(k);
                    break;
                case CacheType.Redis:
                    v = WebCore.Cache.Redis.Instance.Get<string>(k);
                    break;
                default:
                    v = WebCore.Cache.All.Instance.Get<string>(k);
                    break;
            }
            return v;
        }

        /// <summary>
        /// $cache.set("key",123,60,1) // Expire 60 seconds, Redis:1, Cache{ Memory = 0, Redis, Default }
        /// </summary>
        /// <param name="args"></param>
        public void set(params object[] args)
        {
            var length = args.Length;
            if (length < 2)
                return;

            string k = args[0].ToString();
            if (string.IsNullOrEmpty(k)) return;

            string v = (args[1] as ScriptObject != null) ? JsonConvert.SerializeObject(args[1], NewtonsoftJson.Converters) : args[1].ToString();

            int expire = 0, index = 2;
            if (length > 2 && int.TryParse(args[2].ToString(), out int s) && s >= 0)
            {
                expire = s;
                index = 3;
            }

            Set(ArgTypes(index), k, v, expire);
        }

        /// <summary>
        /// Add Cache
        /// </summary>
        /// <param name="types"></param>
        /// <param name="k"></param>
        /// <param name="v"></param>
        /// <param name="expire"></param>
        public static void Set(CacheType types, string k, string v, int expire = 3600 * 24)
        {
            if (string.IsNullOrEmpty(k)) return;
            switch (types)
            {
                case CacheType.Memory:
                    WebCore.Cache.Memory.Instance.Add(k, v, expire);
                    break;
                case CacheType.Redis:
                    WebCore.Cache.Redis.Instance.Add(k, v, expire);
                    break;
                default:
                    WebCore.Cache.All.Instance.Add(k, v, expire);
                    break;
            }
        }

        /// <summary>
        /// $cache.del("key",2) // Default:2, Cache{ Memory = 0, Redis, Default }
        /// </summary>
        /// <param name="args"></param>
        public void del(params object[] args)
        {
            var length = args.Length;
            if (length == 0)
                return;

            string k = args[0].ToString();
            if (string.IsNullOrEmpty(k)) return;

            Del(ArgTypes(1), k);
        }

        /// <summary>
        /// Delete Cache
        /// </summary>
        /// <param name="types"></param>
        /// <param name="k"></param>
        public static void Del(CacheType types, string k)
        {
            if (string.IsNullOrEmpty(k)) return;
            switch (types)
            {
                case CacheType.Memory:
                    WebCore.Cache.Memory.Instance.Remove(k);
                    break;
                case CacheType.Redis:
                    WebCore.Cache.Redis.Instance.Remove(k);
                    break;
                default:
                    WebCore.Cache.All.Instance.Remove(k);
                    break;
            }
        }
    }
}
