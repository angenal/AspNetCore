using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using NATS.Services.Util;
using Newtonsoft.Json;

namespace NATS.Services.V8Script
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class JS_Redis
    {
        readonly Redis.List RedisList;

        readonly V8ScriptEngine Engine;

        readonly string Prefix;
        readonly string Subject;

        public string prefix => Prefix;
        public string subject => Subject;

        public JS_Redis(RedisConfig config, V8ScriptEngine engine, string prefix, string subject)
        {
            RedisList = new Redis.List(new RedisConfiguration(config.Addr) { Password = config.Password, DefaultDatabase = config.Db }.ToString());
            Engine = engine;
            Prefix = prefix;
            Subject = subject;
        }

        /// <summary>
        /// $redis.get("key")
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

            string code = RedisList.Get(k);
            if (string.IsNullOrEmpty(code)) return null;

            return Engine.Evaluate(JS.SecurityCode(code));
        }

        /// <summary>
        /// $redis.set("key",123,60)
        /// </summary>
        /// <param name="args"></param>
        public bool set(params object[] args)
        {
            var length = args.Length;
            if (length < 2)
                return false;

            string k = args[0].ToString();
            if (string.IsNullOrEmpty(k)) return false;

            string v = (args[1] as ScriptObject != null) ? JsonConvert.SerializeObject(args[1], NewtonsoftJson.Converters) : args[1].ToString();

            int expire = 0;
            if (length > 2 && int.TryParse(args[2].ToString(), out int s) && s >= 0)
            {
                expire = s;
            }

            return RedisList.Set(k, v, expire);
        }

        /// <summary>
        /// $redis.push("key",123,60)
        /// </summary>
        /// <param name="args"></param>
        public void push(params object[] args)
        {
            var length = args.Length;
            if (length < 2)
                return;

            string k = args[0].ToString();
            if (string.IsNullOrEmpty(k)) return;

            string v = (args[1] as ScriptObject != null) ? JsonConvert.SerializeObject(args[1], NewtonsoftJson.Converters) : args[1].ToString();

            int expire = 0; // 3600 * 24
            if (length > 2 && int.TryParse(args[2].ToString(), out int s) && s >= 0)
            {
                expire = s;
            }

            RedisList.Push(k, v, expire);
        }

        /// <summary>
        /// var list = $redis.pop("key",10)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object pop(params object[] args)
        {
            var length = args.Length;
            if (length == 0)
                return null;

            string k = args[0].ToString();
            if (string.IsNullOrEmpty(k)) return null;

            int size = 1;
            if (length > 1 && int.TryParse(args[1].ToString(), out int s) && s >= 0)
            {
                size = s;
            }

            var list = RedisList.Pop(k, size);
            if (list.Count == 0) return null;

            var code = "[" + string.Join(",", list) + "]";

            return Engine.Evaluate(JS.SecurityCode(code));
        }

        /// <summary>
        /// var seconds = $redis.expire("key")
        /// var ok = $redis.expire("key", 60)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object expire(params object[] args)
        {
            var length = args.Length;
            if (length == 0)
                return false;

            string k = args[0].ToString();
            if (string.IsNullOrEmpty(k)) return false;

            if (length > 1 && int.TryParse(args[1].ToString(), out int s) && s >= 0)
            {
                return RedisList.Expire(k, s);
            }
            else
            {
                var t = RedisList.Expire(k);
                return t.HasValue ? (int)t.Value.TotalSeconds : -2;
            }
        }

        /// <summary>
        /// var seconds = $redis.idle("key")
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object idle(params object[] args)
        {
            var length = args.Length;
            if (length == 0)
                return -1;

            string k = args[0].ToString();
            if (string.IsNullOrEmpty(k)) return -2;

            var t = RedisList.Idle(k);
            return t.HasValue ? (int)t.Value.TotalSeconds : -2;
        }

        /// <summary>
        /// var ok = $redis.del("key")
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object del(params object[] args)
        {
            var length = args.Length;
            if (length == 0)
                return false;

            string k = args[0].ToString();
            if (string.IsNullOrEmpty(k)) return false;

            return RedisList.Delete(k);
        }
    }
}
