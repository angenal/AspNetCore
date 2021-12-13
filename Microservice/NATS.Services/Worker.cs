using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NATS.Services
{
    public class Worker : BackgroundService
    {
        /// <summary>
        /// 状态：0.默认状态 1.正在退出 2.正在停止
        /// </summary>
        private volatile int status;
        private readonly ILogger<Worker> _logger;
        private readonly IHostApplicationLifetime _host;

        public Worker(ILogger<Worker> logger, IHostApplicationLifetime host)
        {
            _logger = logger;
            _host = host;
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
                _logger.LogInformation("正在进行: {time}", DateTimeOffset.Now);
                await Task.Delay(5000, stoppingToken); // 间隔5秒打印一次
            }
        }

        /// <summary>
        /// 正在退出
        /// </summary>
        public void OnStopping()
        {
            status = 1;
            _logger.LogInformation("正在退出");
        }

        /// <summary>
        /// 正在停止
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            status = 2;
            _logger.LogInformation("正在停止");

            return base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// 开始启动
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            // 注册:在退出前需要完成的操作
            _host.ApplicationStopping.Register(OnStopping);
            _logger.LogInformation("开始启动");

            return base.StartAsync(cancellationToken);
        }
    }
}
