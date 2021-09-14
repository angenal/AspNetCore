using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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

            // Create logger
            ConfigureLogging(args);

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

        // Create logger, reference https://github.com/serilog/serilog-settings-configuration/blob/dev/sample/Sample
        /// <summary></summary>
        static void ConfigureLogging(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
            string basePath = Directory.GetCurrentDirectory(), serilogAppsettings = "appsettings.serilog.json";
            if (File.Exists(Path.Join(basePath, serilogAppsettings)))
            {
                var builder = new ConfigurationBuilder().SetBasePath(basePath)
                    .AddJsonFile(serilogAppsettings, optional: false, reloadOnChange: true);

                if ("Development".Equals(environmentName, StringComparison.OrdinalIgnoreCase))
                    builder.AddUserSecrets(Assembly.GetExecutingAssembly());

                var configuration = builder.Build();
                Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration)
                    .CreateLogger();
            }
            else
            {
                var builder = new ConfigurationBuilder().SetBasePath(basePath)
                    .AddEnvironmentVariables("DOTNET_")
                    .AddCommandLine(args)
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{environmentName}.json", true, true);

                if ("Development".Equals(environmentName, StringComparison.OrdinalIgnoreCase))
                    builder.AddUserSecrets(Assembly.GetExecutingAssembly());

                var configuration = builder.Build();
                Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateBootstrapLogger();
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// <summary></summary>
        static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddHostedService<Worker>();
        }

        /// <summary></summary>
        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => builder.ClearProviders())
                .ConfigureServices(ConfigureServices) // Use this method to add services to the container.
                .UseWindowsService(options => options.ServiceName = typeof(Program).Namespace) // Sets the host lifetime to WindowsServiceLifetime.
                .UseSerilog();
    }
}
