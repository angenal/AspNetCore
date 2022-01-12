using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebFramework.Data;
using WebInterface;

namespace WebFramework
{
    /// <summary>
    /// Api ControllerBase Extensions
    /// </summary>
    public class ApiController : ControllerBase
    {
        /// <summary>
        /// Current user info
        /// </summary>
        public Session user;

        /// <summary>
        /// new SqlSugarClient
        /// </summary>
        public SqlSugarClient db => _db ??= SQLServerDb.DefaultConnection.NewSqlSugarClient(":", SqlSugarClientDebug);
        private SqlSugarClient _db;
        private void SqlSugarClientDebug(string sql)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(sql);
#endif
            var trace = HttpContext.TraceIdentifier;
            if (trace != null) HttpContext.Items.TryAdd($"{trace}sql", sql);
        }

        /// <summary></summary>
        public ApiController() { }

        /// <summary></summary>
        protected BadRequestObjectResult Error(string title, int status = 400)
        {
            return BadRequest(new ErrorJsonResultObject { Status = status, Title = title });
        }
        /// <summary></summary>
        protected BadRequestObjectResult Error(string title, string detail, int status = 400)
        {
            return BadRequest(new ErrorJsonResultObject { Status = status, Title = title, Detail = detail });
        }

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
