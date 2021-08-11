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
            // Must be modified, default C:\WINDOWS\system32
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            var basePath = Directory.GetCurrentDirectory();

            // Gets environment name
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

            var builder = new ConfigurationBuilder().SetBasePath(basePath)
                .AddEnvironmentVariables("DOTNET_")
                .AddCommandLine(args)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", true, true);

            if ("Development".Equals(environmentName, StringComparison.OrdinalIgnoreCase))
                builder.AddUserSecrets(Assembly.GetExecutingAssembly());

            var configuration = builder.Build();

            // Create default logger
            // Use config file https://github.com/serilog/serilog-settings-configuration/blob/dev/sample/Sample
            if (File.Exists(Path.Join(basePath, "appsettings.serilog.json")))
            {
                Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(new ConfigurationBuilder().SetBasePath(basePath)
                    .AddJsonFile("appsettings.serilog.json", optional: false, reloadOnChange: true).Build()).CreateLogger();
            }
            else
            {
                Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateBootstrapLogger();
            }

            var watch = Stopwatch.StartNew();
            var separator = new string('-', 20);

            try
            {
                Log.Information($"{separator} Start run host {separator} ");

                CreateHostBuilder(args).Build().Run();

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
                .UseWindowsService() // Sets the host lifetime to WindowsServiceLifetime
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                })
                .UseSerilog();
    }
}
