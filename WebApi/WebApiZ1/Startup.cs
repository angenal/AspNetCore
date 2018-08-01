using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebFramework.Authentications;
using WebFramework.Configurations;
using WebFramework.ORM;
using WebFramework.Startups;
using WebApiZ1.Models;
using WebCore.Data.DTO;

namespace WebApiZ1
{
    /// <summary>
    /// 启动WebHost
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="env"></param>
        public Startup(IHostingEnvironment env)
        {
            var builder = AppSettings.FromConfigurationBuilder(env);
            Configuration = builder.Build();

            NSwagContainer = new NSwagContainer(Configuration, typeof(Startup).Assembly);
            ECachingContainer = new EasyCachingContainer(Configuration);
            ApiAuthorized = new ApiAuthorized(Configuration);
        }
        /// <summary>
        /// 配置
        /// </summary>
        public IConfiguration Configuration { get; }
        /// <summary>
        /// NSwag配置
        /// </summary>
        public NSwagContainer NSwagContainer { get; }
        /// <summary>
        /// 缓存配置
        /// </summary>
        public EasyCachingContainer ECachingContainer { get; }
        /// <summary>
        /// api认证
        /// </summary>
        public ApiAuthorized ApiAuthorized { get; }


        /// <summary>
        /// 管理服务容器
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // IISOptions
            services.Configure<IISOptions>(opt =>
            {

            });

            // 数据库
            services.AddDbContext<ValuesContext>(opt =>
            {
                opt.UseInMemoryDatabase("Values");
            });
            services.AddDbContextOfSqlSugar<ValuesContextOfSqlSugar>(opt =>
            {
                opt.IsAutoCloseConnection = true;
                opt.DbType = SqlSugar.DbType.SqlServer;
                opt.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
            });

            // Mvc
            services.AddMvc();//.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            // 缓存
            ECachingContainer.ConfigureServices(services);
            // api认证
            ApiAuthorized.ConfigureServices(services);
            ApiAuthorized.ConfigAppAsync(() =>
            {
                return new List<IAppInfo>
                {
                    new AppInfo()
                    {
                        appid = "FA9F0K19",
                        name = "ApiAuth1",
                        secret = "IFHA89IE"
                    }
                };
            });
        }
        /// <summary>
        /// Http处理Pipeline中间件Middleware
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // 日志
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // 环境
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // 启用静态资源访问
            app.UseStaticFiles();
            // 导航到 http://localhost:<port>/swagger 查看 Swagger UI
            // 导航到 http://localhost:<port>/swagger/v1/swagger.json 查看 Swagger 规范
            NSwagContainer.Configure(app, env);

            // 缓存
            ECachingContainer.Configure(app, env);

            // 认证
            ApiAuthorized.Configure(app, env, loggerFactory);

            // 启用Https
            if ("true" == Configuration["Https"]) app.UseHttpsRedirection();

            // Mvc
            app.UseMvc();
        }
    }
}
