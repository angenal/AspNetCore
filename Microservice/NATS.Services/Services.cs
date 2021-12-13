using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NATS.Services
{
    internal static class Services
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// <summary>Use this method to add services to the container.</summary>
        public static void Configure(HostBuilderContext context, IServiceCollection services)
        {
            services.AddHostedService<Worker>();
        }
    }
}
