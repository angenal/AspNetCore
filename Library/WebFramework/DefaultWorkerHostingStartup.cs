using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(WebFramework.DefaultWorkerHostingStartup))]
namespace WebFramework
{
    /// <summary></summary>
    public class DefaultWorkerHostingStartup : IHostingStartup
    {
        /// <summary></summary>
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddHostedService<DefaultWorker>();
            });
        }
    }
}
