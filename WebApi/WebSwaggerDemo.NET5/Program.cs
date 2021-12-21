using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace WebSwaggerDemo.NET5
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // 初始化
            WebCore.Assemblies.All = System.Runtime.Loader.AssemblyLoadContext.Default.Assemblies;
            // 系统入口:初始化
            WebCore.Main.Init();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
