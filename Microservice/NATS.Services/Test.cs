using Microsoft.ClearScript;
using Serilog;
using System;
using System.IO;

namespace NATS.Services
{
    sealed class Test
    {
        /// <summary>
        /// 调试命令行参数：-i 5 -t natsql.test.json
        /// </summary>
        /// <param name="flags"></param>
        public static void Run(Flags flags)
        {
            // 1.解析 natsql.yaml
            var config = Config.Parse(flags.Config);

            // 2.初始化配置
            var n = config.Init(0, flags.CreateInterval);
            if (n == 0) return;

            // 3.执行 natsql.js
            foreach (var item in Config.Items)
            {
                // test json data
                string testFile = $"{item.Dir}/{flags.Test}";
                if (!File.Exists(testFile)) continue;
                string testJson = File.ReadAllText(testFile);
                Log.Debug($"[{item.Subject}] <- test json file: {testFile}");

                try
                {
                    // test js function
                    var res = item.Invoke(testJson);
                    if (!(res is Undefined))
                    {
                        // execute sql command
                        if ("String" == res.GetType().Name && res.ToString().Length >= 20)
                            res = item.JS.Database.x(res) + " records affected database";
                        Log.Debug($"[{item.Subject}] <- @sql: {res}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"[{item.Subject}] <- @dql: error");
                }
            }

            Log.Debug("press [Ctrl+C] to quit...");

            WebCore.Exit.AddAction(new Action(() =>
            {
                foreach (var item in Config.Items) item.Close().JsHandler?.Wait(true);
                //Config.JsCronScheduler.Shutdown();
                Log.Debug("run finished and exit.");
            }));

            WebCore.Exit.AddAction(Log.CloseAndFlush);
            WebCore.Exit.Wait();
        }
    }
}
