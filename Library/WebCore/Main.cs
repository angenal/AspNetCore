using FluentScheduler;
using System;
using System.IO;
using System.Text;

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
        }
    }
}
