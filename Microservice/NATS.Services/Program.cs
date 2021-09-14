using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NATS.Services
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Init
            WebCore.Main.Init();

            // Create default logger, reference https://github.com/serilog/serilog-settings-configuration/blob/dev/sample/Sample
            string basePath = Directory.GetCurrentDirectory(), serilogAppsettings = "appsettings.serilog.json";
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
            if (File.Exists(Path.Join(basePath, serilogAppsettings)))
            {
                var configuration = new ConfigurationBuilder().SetBasePath(basePath)
                    .AddJsonFile(serilogAppsettings, optional: false, reloadOnChange: true)
                    .Build();
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                // Sets the host lifetime to WindowsServiceLifetime
                .UseWindowsService(options => options.ServiceName = typeof(Program).Namespace)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                })
                .UseSerilog();
    }
}
