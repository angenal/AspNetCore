using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace WebFramework.WebHost
{
    public sealed class Builder
    {
        /// <summary>
        /// 创建WebHost for IIS | Kestrel
        /// </summary>
        /// <typeparam name="TStartup">管理服务容器 Http处理Pipeline中间件Middleware</typeparam>
        /// <param name="args">命令参数</param>
        /// <returns></returns>
        public static IWebHost BuildUseStartup<TStartup>(string[] args) where TStartup : class
        {
            IConfiguration configuration = new ConfigurationBuilder() // 开始配置
                .AddCommandLine(args) // 添加对命令参数的支持
                .SetBasePath(Directory.GetCurrentDirectory()) // 设置工作目录 | 根目录
                .AddJsonFile("host.json", optional: true) // 告诉Kestrel读取config文件:{ "urls": "http://localhost:5002" }
                .Build();

            return Microsoft.AspNetCore.WebHost.CreateDefaultBuilder(args)
                .UseStartup<TStartup>()
                //.UseIISIntegration()
                //.UseKestrel(o => o.Listen(System.Net.IPAddress.Loopback, 5002, c => c.UseHttps("WebRootCA.pfx", "123456")))
                //.UseUrls("http://*:0")//http://localhost:5002;http://localhost:5003
                .UseConfiguration(configuration)//配置完成
                .Build();
        }

    }
}
