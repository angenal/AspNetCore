using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WebCore;
using WebCore.Models.DTO;
using WebInterface;
using WebSwaggerDemo.NET5.Models;

namespace WebSwaggerDemo.NET5.Controllers
{
    /// <summary>
    /// 任务 控制器
    /// </summary>
    [ApiController]
    [ApiGroup(GroupSample.Demo)]
    [Route("api/[controller]/[action]")]
    public class TaskController : Controller
    {
        private readonly ITaskManager taskManager;
        private readonly ITaskExecutor taskExecutor;

        public TaskController(ITaskManager taskManager, ITaskExecutor taskExecutor)
        {
            this.taskManager = taskManager;
            this.taskExecutor = taskExecutor;
        }

        /// <summary>
        /// 查询
        /// </summary>
        [HttpGet]
        public virtual Result Get([FromQuery] IdInput input)
        {
            string key = $"{input.Id}";

            return Result.Success(new
            {
                input.Id,
                Memory = WebCore.Cache.Memory.Instance.Get<string>(key),
                Redis = WebCore.Cache.Redis.Instance.Get<string>(key)
            });
        }

        /// <summary>
        /// 提交
        /// </summary>
        [HttpPost("{expire:int}")]
        public Result Set([FromBody] IdInput input, [FromRoute] int expire = 10)
        {
            string key = $"{input.Id}";
            string value = DateTime.Now.ToDateTimeString();

            // 不可能阻塞(输出后执行)
            taskExecutor.Execute(state =>
            {
                dynamic input = state.ToDynamic();
                Trace.WriteLine($"In: {DateTime.Now.Ticks}  {{ key: '{input.key}', value: '{input.value}' }}");
            }, new { key, value }.ToDynamic(), true);

            // 有可能阻塞(输出前执行)
            taskManager.Enqueue(async token =>
            {
                Trace.WriteLine($"In.Queue: {DateTime.Now.Ticks}");
                if (token.IsCancellationRequested) return;

                WebCore.Cache.Memory.Instance.Set(key, value, expire);
                await Task.CompletedTask;
            });
            Trace.WriteLine($"Out.Queue: {DateTime.Now.Ticks}");

            // 不可能阻塞(输出后执行) 可提高响应速度
            taskManager.Enqueue(() =>
            {
                Trace.WriteLine($"In.Job: {DateTime.Now.Ticks}");

                WebCore.Cache.Redis.Instance.Set(key, value, expire);
            });
            Trace.WriteLine($"Out.Job: {DateTime.Now.Ticks}");

            return Result.Success(new { input.Id, value, t = DateTime.Now.Ticks.ToString() });
        }
    }
}
