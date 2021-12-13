using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Reflection;

namespace NATS.Services
{
    /// <summary>
    /// Init logger, reference https://github.com/serilog/serilog-settings-configuration/blob/dev/sample/Sample
    /// </summary>
    internal static class Logger
    {
        /// <summary></summary>
        public static void Init(string[] args)
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
    }
}
