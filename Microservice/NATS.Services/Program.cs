using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
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

            try
            {
                // test log.
                // var position = new { Latitude = 25, Longitude = 134 };
                // var elapsed = 34;
                // Log.Information("Processed {@Position} in {Elapsed:000} ms.", position, elapsed);

                var separator = new string('-', 30);

                Log.Information($"{separator} Starting host {separator} ");

                CreateHostBuilder(args).Build().Run();

                Log.Information($"{separator} Exit host {separator} ");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
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
