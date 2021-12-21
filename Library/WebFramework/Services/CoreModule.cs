using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebCore;
using WebInterface;

namespace WebFramework.Services
{
    /// <summary>
    /// WebCore Module
    /// </summary>
    public static class CoreModule
    {
        /// <summary>
        /// Register WebCore services
        /// </summary>
        public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<ITaskManager>(_ => TaskManager.Default);

            return services;
        }
    }
}
