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
            // BackgroundService: TaskService
            services.AddHostedService<TaskService>();
            // FluentScheduler: TaskManager
            services.AddSingleton<ITaskManager>(TaskManager.Default);
            // Allow to raise a task completion source with minimal costs and attempt to avoid stalls due to thread pool starvation.
            services.AddSingleton<ITaskExecutor>(TaskExecutor.Default);

            return services;
        }
    }
}
