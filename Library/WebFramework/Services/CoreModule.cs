using Microsoft.AspNetCore.Builder;
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
            // Start Workflow Core
            // https://github.com/zhenl/ZL.WorflowCoreDemo/blob/master/ZL.WorkflowCoreDemo.UserWorkflow/Program.cs
            //var host = app.ApplicationServices.GetService<IWorkflowHost>();
            //Debug.WriteLine("Register workflow...");

            //host.RegisterWorkflow<HumanWorkflow>();

            //Debug.WriteLine("Starting workflow...");
            //host.Start();

            //var workflowId = host.StartWorkflow("HumanWorkflow").Result;
            //host.PublishUserAction(workflowId, "", 0);

            return app;
        }
    }
}
