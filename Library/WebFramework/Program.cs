using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using WebFramework.Services;

namespace WebFramework
{
    public static class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                //.ConfigureAppConfiguration((context, builder) =>
                //{
                //    //The following configuration has been loaded automatically by default
                //    builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true);
                //    builder.AddEnvironmentVariables();//builder.AddEnvironmentVariables("ASPNETCORE_");
                //    builder.AddCommandLine(args);
                //})
                .ConfigureLogging()
                .ConfigureWebHostDefaults(builder => builder.UseStartup<ProgramStartup>().UseSentryMonitor());
        }
    }
}
