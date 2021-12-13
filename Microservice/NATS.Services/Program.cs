using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;

namespace NATS.Services
{
    /// <summary></summary>
    public class Program
    {
        /// <summary></summary>
        public static void Main(string[] args)
        {
            // Init
            WebCore.Main.Init();

            // Logger
            Logger.Init(args);

            // Performance mark [start]
            var watch = Stopwatch.StartNew();
            var separator = new string('-', 20);

            try
            {
                Log.Information($"{separator} Start run host {separator} ");

                CreateHostBuilder(args).Build().Run();

                // Performance mark [stop]
                watch.Stop();
                Log.Information($"{separator} Normal exit host {watch.Elapsed} elapsed {separator} ");
            }
            catch (Exception ex)
            {
                watch.Stop();
                Log.Information($"{separator} Abnormal exit host {watch.Elapsed} elapsed {separator} ");
                Log.Fatal(ex, " host error ");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary></summary>
        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => builder.ClearProviders())
                .ConfigureServices(Services.Configure) // Use this method to add services to the container.
                .UseWindowsService(options => options.ServiceName = typeof(Program).Namespace) // Sets the host lifetime to WindowsServiceLifetime.
                .UseSerilog();
    }
}
