using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace WebDotnetCore.db.sqlservr
{
    class Program
    {
        static void Main(string[] args)
        {
            IWebHost host = BuildWebHost(args);

            //初始化数据库
            using (var scope = host.Services.CreateScope())
            {
                try
                {
                    //YzDbContext.Initialize(scope);//异常?
                }
                catch (Exception e)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError($"Exception: {e.InnerException?.Message ?? e.Message}");
                }
            }

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                //.UseIISIntegration()
                //.UseKestrel(o => o.Listen(System.Net.IPAddress.Loopback, 5002, c => c.UseHttps("WebRootCA.pfx", "123456")))
                //.UseUrls("http://*:0")//http://localhost:5002;http://localhost:5003
                .UseConfiguration(new ConfigurationBuilder()//开始配置
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)//设置工作目录|根目录
                .AddCommandLine(args)//添加对命令参数的支持
                .AddJsonFile("host.json", optional: true)//告诉Kestrel读取config文件
                .Build())//配置完成
                .Build();
    }
}
