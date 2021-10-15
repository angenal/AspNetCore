using FluentScheduler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebCore
{
    /// <summary>
    /// 系统入口
    /// </summary>
    public sealed class Main
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            // Must be modified, default C:\WINDOWS\system32
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            // to avoid this error add the nuget package below: System.NotSupportedException: No data is available for encoding 1250.
            // for information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method.
            // https://www.nuget.org/packages/System.Text.Encoding.CodePages
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Initializes the Date.
            Date.Init();

            // Initializes the job manager with the jobs to run and starts it.
            JobManager.Initialize();
            // Use UTC time rather than local time. It's recommended to call this method before
            // any other library interaction to avoid mixed dates.
            JobManager.UseUtcTime();

            // Occurs when the default application domain's parent process exits.
            AppDomain.CurrentDomain.ProcessExit += Exit;
        }
        /// <summary>
        /// 应用程序退出前执行
        /// </summary>
        public static readonly ICollection<Action> OnExit = new List<Action>();
        /// <summary>
        /// https://docs.microsoft.com/en-us/dotnet/api/system.appdomain.processexit
        /// </summary>
        private static void Exit(object sender, EventArgs e)
        {
            const int seconds = 2;
            foreach (Action action in OnExit) Task.Factory.StartNew(action, new CancellationTokenSource(TimeSpan.FromSeconds(seconds)).Token);
            JobManager.StopAndBlock();
        }
    }
}
