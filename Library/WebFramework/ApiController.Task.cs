using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebInterface;

namespace WebFramework
{
    /// <summary>
    /// ApiController Extensions ITaskExecutor, ITaskManager
    /// </summary>
    public partial class ApiController
    {
        /// <summary>
        /// 运行非阻塞的队列任务(输出后执行)可提高响应速度
        /// </summary>
        /// <param name="callback">任务: state => { dynamic input = state.ToDynamic(); }</param>
        /// <param name="state">任务参数: new { value }.ToDynamic()</param>
        /// <param name="laterOnEvent">是否为非阻塞的任务</param>
        protected void Run(WaitCallback callback, object state, bool laterOnEvent = true)
        {
            var taskExecutor = HttpContext.RequestServices.GetService<ITaskExecutor>();
            if (taskExecutor == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <ITaskExecutor> from the ServiceProvider.");
                return;
            }
            taskExecutor.Execute(callback, state, laterOnEvent);
        }
        /// <summary>
        /// 运行非阻塞的队列任务(输出后执行)可提高响应速度
        /// </summary>
        /// <param name="task">任务: () => { 可访问外部变量 }</param>
        protected void Run(Action task)
        {
            var taskManager = HttpContext.RequestServices.GetService<ITaskManager>();
            if (taskManager == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <ITaskManager> from the ServiceProvider.");
                return;
            }
            taskManager.Enqueue(task);
        }
        /// <summary>
        /// 运行高效的队列任务(有可能阻塞)可提高响应速度
        /// </summary>
        /// <param name="task">任务: async token => { await Task.CompletedTask; }</param>
        protected void Run(Func<CancellationToken, Task> task)
        {
            var taskManager = HttpContext.RequestServices.GetService<ITaskManager>();
            if (taskManager == null)
            {
                System.Diagnostics.Debug.WriteLine($"Not found service of type <ITaskManager> from the ServiceProvider.");
                return;
            }
            taskManager.Enqueue(task);
        }
    }
}
