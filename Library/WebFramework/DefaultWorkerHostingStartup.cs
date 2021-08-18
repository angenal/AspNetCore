using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(WebFramework.DefaultWorkerHostingStartup))]
namespace WebFramework
{
    public class DefaultWorkerHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddHostedService<DefaultWorker>();
            });
        }
    }
}
