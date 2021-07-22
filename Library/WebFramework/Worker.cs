using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WebFramework
{
    /// <summary>
    /// Default Worker for Background Service.
    /// </summary>
    public class Worker : BackgroundService
    {
        /// <summary>
        /// Interval Time
        /// </summary>
        public static TimeSpan Interval = TimeSpan.FromSeconds(2);
        /// <summary>
        /// Interval Actions
        /// </summary>
        public static List<Action> Actions = new List<Action>();

        readonly IHostApplicationLifetime host;
        readonly ILogger L;

        public Worker(IHostApplicationLifetime host, ILogger logger = null)
        {
            this.host = host;
            if (logger != null) L = logger.ForContext<Worker>();
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            L.Debug("Worker Start");
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
                        foreach (Action action in Actions) action?.Invoke();
                        //L.Debug("Worker Run: {time}", DateTimeOffset.Now);
                    }
                    catch (Exception ex)
                    {
                        L.Error(ex.Message);
                    }

                    await Task.Delay(Interval, stoppingToken);
                }
            }
            finally
            {
                host.StopApplication();
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            L.Debug("Worker Stop");
            return base.StopAsync(cancellationToken);
        }
    }
}
