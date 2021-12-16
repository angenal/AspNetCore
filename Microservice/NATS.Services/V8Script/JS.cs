using HtmlAgilityPack;
using Microsoft.ClearScript.V8;
using NATS.Client;
using System;
using System.Xml;
using WebCore;

namespace NATS.Services.V8Script
{
    /// <summary>
    /// V8 JavaScript Runtime.
    /// https://microsoft.github.io/ClearScript/Examples/Examples.html
    /// https://microsoft.github.io/ClearScript/Tutorial/FAQtorial.html
    /// </summary>
    public sealed class JS
    {
        /// <summary>
        /// Identity.
        /// </summary>
        public readonly uint Id;

        /// <summary>
        /// Represents an instance of the V8 JavaScript engine.
        /// </summary>
        public readonly V8ScriptEngine Engine;

        /// <summary>
        /// Compiled script.
        /// </summary>
        public readonly Microsoft.ClearScript.V8.V8Script Script;

        public readonly JS_Db Database;
        public readonly JS_Nats NatsObject;
        public readonly JS_Cache CacheObject;
        public readonly JS_Redis RedisObject;

        /// <summary>
        /// V8 JavaScript Runtime.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="extensions"></param>
        /// <param name="executing"></param>
        /// <param name="enableDebugging"></param>
        public JS(string script, DbConfig dbConfig, RedisConfig redisConfig, IConnection connection, string prefix, string subject, bool extensions = true, bool executing = true, bool enableDebugging = false)
        {
            Id = script.Crc32();

            //var constraints = new V8RuntimeConstraints
            //{
            //    MaxNewSpaceSize = 16,
            //    MaxOldSpaceSize = 8,
            //};
            Engine = enableDebugging ? new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging) : new V8ScriptEngine();

            if (extensions)
            {
                Engine.AddHostType(typeof(DateTime));
                Engine.AddHostType(typeof(TimeSpan));
                Engine.AddHostType(typeof(XmlDocument));
                Engine.AddHostType(typeof(HtmlDocument));
                Engine.AddHostType(typeof(HtmlWeb));
                Engine.AddHostType(typeof(HtmlNode));
                Engine.AddHostType(typeof(HtmlNodeCollection));
                //Engine.AddHostObject("host", new HostFunctions());
                //Engine.AddHostObject("type", new HostTypeCollection("mscorlib", "System", "System.Core"));
                //JS_Script:[this] type extensions
                Engine.AddHostType(typeof(JS_Script));
                //JS_Console:[console] static functions
                Engine.AddHostType("console", typeof(JS_Console));

                //JS_Ajax:[$] object methods
                Add("$", new JS_Ajax(Engine));
                Add("$db", Database = new JS_Db(dbConfig, Engine, prefix, subject));
                Add("$nats", NatsObject = new JS_Nats(connection, prefix, subject));
                Add("$cache", CacheObject = new JS_Cache(redisConfig, Engine, prefix, subject));
                Add("$redis", RedisObject = new JS_Redis(redisConfig, Engine, prefix, subject));
                Engine.AddHostExtensions();
            }

            // Creates a compiled script.
            Script = Engine.Compile(script);
            if (executing) Engine.Execute(Script);
        }

        public void Add(string itemName, object target) => Engine.AddHostObject(itemName, target);
        //public void Execute(string script) => Engine.Execute(script);
        //public object Eval(string code) => Engine.Evaluate(SecurityCode(code));
        //public object Invoke(string funcName, params object[] args) => Engine.Invoke(funcName, args);
        //public object Invoke(string funcName, string codeEvalToArgs) => Engine.Invoke(funcName, Eval(codeEvalToArgs));
        public static string SecurityCode(string code) => code != null && code.StartsWith("{") ? $"_{new Random().Next(100, 9999)}=" + code : code;
    }
}
