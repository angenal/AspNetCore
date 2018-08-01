using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebDotnetCore.db.sqlservr.yz.Models;
using WebFramework.Startups;

namespace WebDotnetCore.db.sqlservr
{
    public class Startup
    {
        /// <summary>
        /// Autofac IContainer
        /// </summary>
        public readonly AutofacContainer AutofacContainer;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Config db ConnectionString in appsettings.json

            // �����ݿ�������MODEL
            // PM> Install-Package Microsoft.EntityFrameworkCore.SqlServer
            // PM> Install-Package Microsoft.EntityFrameworkCore.Tools
            // PM> Scaffold-DbContext "Server=(localdb)\mssqllocaldb;Database=DbName;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models

            // �Ӵ���MODEL�����ݿ⣬���ơ�Initial��������ģ����ڶ�Ǩ���ļ���������
            // PM> Add-Migration Initial
            // PM> Update-Database

            // WEBҳ�������������
            // PM> Install-Package Microsoft.VisualStudio.Web.CodeGeneration.Design -Version 2.0.3
            //CMD> dotnet aspnet-codegenerator razorpage -m Movie -dc MovieContext -udl -outDir Pages\Movies --referenceScriptLibraries
            services.AddDbContext<YzDbContext>(o =>
                o.UseSqlServer(Configuration.GetConnectionString("YZConnectionString"),
                    b => b.MigrationsAssembly("WebDotnetCore.db.sqlservr") // PM> Add-Migration ��ǰ��Ŀִ��Ǩ��
            ), contextLifetime: ServiceLifetime.Singleton, optionsLifetime: ServiceLifetime.Singleton);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Run(async context =>
            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync($"<center><br><h2>��ǰ�����ڣ�{env.EnvironmentName} of {env.ApplicationName}</h2></center>");
            });
        }
    }
}
