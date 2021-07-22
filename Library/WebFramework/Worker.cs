using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebFramework
{
    public class Worker : BackgroundService
    {
        private readonly IHostApplicationLifetime host;
        private readonly ILogger Logger;

        public Worker(IHostApplicationLifetime host, ILogger logger)
        {
            this.host = host;
            Logger = logger.ForContext<Worker>();
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.Debug("Worker Start ...");
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        Logger.Debug("Worker Run: {time}", DateTimeOffset.Now);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
            finally
            {
                host.StopApplication();
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.Debug("Worker Stop ...");
            return base.StopAsync(cancellationToken);
        }
    }
}
