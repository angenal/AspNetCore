using CommandLine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace NATS.Services
{
    /// <summary></summary>
    public class Program
    {
        /// <summary></summary>
        public static void Main(string[] args)
        {
            if (args.Length == 0) { Flags.NotParsed(null); return; }
            var result = Parser.Default.ParseArguments<Flags>(args).WithNotParsed(Flags.NotParsed);
            if (result is not Parsed<Flags> parsedResult) return;
            Flags flags = parsedResult.Value;

            // Init
            WebCore.Main.Init();

            // Logger
            Logger.Init(args);

            // Run test
            if (!string.IsNullOrEmpty(flags.Test)) { Test.Run(flags); return; }

            // Run subscribes
            Subscribes.Run(flags);
            IHost host = CreateHostBuilder(args).Build();
            host.Start();
            WebCore.Exit.AddAction(host.WaitForShutdown);
            WebCore.Exit.Wait();
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
