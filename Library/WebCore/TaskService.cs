using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebInterface;

namespace WebCore
{
    /// <summary>
    /// 后台任务服务
    /// </summary>
    public class TaskService : BackgroundService
    {
        /// <summary>
        /// 状态：0.默认状态 1.正在退出 2.正在停止
        /// </summary>
        private volatile int status;
        private readonly ILogger<TaskService> _logger;
        private readonly IHostApplicationLifetime _host;
        private readonly ITaskManager _taskManager;

        /// <summary></summary>
        public TaskService(ILogger<TaskService> logger, IHostApplicationLifetime host, ITaskManager taskManager = null)
        {
            _logger = logger;
            _host = host;
            _taskManager = taskManager ?? TaskManager.Default;
        }

        /// <summary>
        /// 正在进行
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested && status == 0)
            {
                //_logger.LogInformation("正在进行: {time}", DateTimeOffset.Now);

                var task = await _taskManager.Dequeue(stoppingToken);

                try
                {
                    await task(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing task service[0]", nameof(task));
                }
            }
        }

        /// <summary>
        /// 正在退出
        /// </summary>
        public void OnStopping(object state)
        {
            status = 1;

            //_logger.LogInformation("正在退出");

            var cancellationToken = (CancellationToken)state;
            if (!cancellationToken.IsCancellationRequested) return;

            var task = _taskManager.Dequeue(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
            while (task != null && status == 1)
            {
                try
                {
                    task(CancellationToken.None).Wait();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing task service[1]", nameof(task));
                }
                finally
                {
                    task = _taskManager.Dequeue(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
        }

        /// <summary>
        /// 正在停止
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            status = 2;

            //_logger.LogInformation("正在停止");

            await base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// 开始启动
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            //_logger.LogInformation("开始启动");

            // 注册:在退出前需要完成的操作
            _host.ApplicationStopping.Register(OnStopping, cancellationToken);

            await base.StartAsync(cancellationToken);
        }
    }
}
