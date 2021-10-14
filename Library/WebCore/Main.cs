using FluentScheduler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

            // Gets all the assemblies is loaded into our current application domain.
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

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

        /// <summary>
        /// Gets all the assemblies is loaded into our current application domain.
        /// </summary>
        public static ICollection<Assembly> Assemblies = new List<Assembly>() { Assembly.GetEntryAssembly() };
        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            var assembly = args.LoadedAssembly;
            if (Assemblies.Contains(assembly)) Assemblies.Remove(assembly);
            Assemblies.Add(assembly);
        }
    }
}
