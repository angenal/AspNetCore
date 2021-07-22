using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(WebFramework.WorkerHostingStartup))]
namespace WebFramework
{
    public class WorkerHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddHostedService<Worker>();
            });
        }
    }
}
