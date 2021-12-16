using Serilog;
using System;
using System.Threading;

namespace NATS.Services
{
    public partial class Subscribes
    {
        /// <summary>
        /// 调试命令行参数：-i 5 -c natsql.yaml
        /// </summary>
        /// <param name="flags"></param>
        public static void Run(Flags flags)
        {
            // 1.解析 natsql.yaml
            config = Config.Parse(flags.Config);

            // 2.初始化配置
            var n = config.Init(created, flags.CreateInterval);
            if (n == 0) return;

            // 3.执行 natsql.js
            created = Config.LatestCreated();

            Log.Information("run started.");
            Log.Debug("press [Ctrl+C] to quit...");

            WebCore.Exit.AddAction(new Action(() =>
            {
                foreach (var item in Config.Items) item.Close().JsHandler?.Wait(true);
                //Config.JsCronScheduler.Shutdown();
                Log.Information("run finished and exit.");
            }));

            if (flags.CreateInterval > 0)
            {
                createThread = new Thread(new ParameterizedThreadStart(Create)) { IsBackground = true };
                createThread.Start(flags.CreateInterval);
            }

            WebCore.Exit.AddAction(Log.CloseAndFlush);
        }

        static long created = 0;
        static Thread createThread;
        static Config config = null;

        static void Create(object obj)
        {
            int loadInterval = Convert.ToInt32(obj), interval = loadInterval * 1000;
            while (true)
            {
                Thread.Sleep(interval);

                var n = config.Init(created, loadInterval, true);
                if (n == 0) continue;

                created = Config.LatestCreated();
            }
        }
    }
}
