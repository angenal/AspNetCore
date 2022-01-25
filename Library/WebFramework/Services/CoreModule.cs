using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using WebCore;
using WebInterface;
using WorkflowCore.Interface;

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
            // Provides extensions for Workflow Core to enable human workflows.
            services.AddWorkflow();
            //services.AddWorkflow(x => x.UseSqlServer(@"Server=.;Database=Workflow;Trusted_Connection=True", true, true));

            return services;
        }

        /// <summary>
        /// Use WebCore Middleware
        /// </summary>
        public static IApplicationBuilder UseCore(this IApplicationBuilder app, IConfiguration config)
        {
            // Register Workflow  https://github.com/zhenl/ZL.WorflowCoreDemo
            var assemblies = System.Runtime.Loader.AssemblyLoadContext.Default.Assemblies;
            var host = app.ApplicationServices.GetRequiredService<IWorkflowHost>();
            app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register(host.Stop);
            var register = host.GetType().GetMethods().FirstOrDefault(m => m.Name == "RegisterWorkflow" && m.GetGenericArguments().Length == 1);
            foreach (Type type in assemblies.GetTypesIs<IWorkflow>()) register.MakeGenericMethod(type).Invoke(host, Array.Empty<object>());
            host.Start();
            // Use ApiController.Workflow  https://www.cnblogs.com/edisonchou/p/lightweight_workflow_engine_for_dotnetcore.html
            //var workflowId = host.StartWorkflow("").Result; host.PublishUserAction(workflowId, "", 0);

            return app;
        }
    }
}
