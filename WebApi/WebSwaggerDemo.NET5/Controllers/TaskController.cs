using Microsoft.AspNetCore.Mvc;
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
        public virtual Result Query([FromQuery] IdInput input)
        {
            return Result.Success(input);
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="sample">查询例子</param>
        [HttpPost]
        public Result Post([FromBody] IdInput sample)
        {
            return Result.Success(sample);
        }
    }
}
