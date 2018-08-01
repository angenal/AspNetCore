using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WebFramework.Startups;
using WebDotnetCore.db.sqlservr.yz.Models;

namespace WebApplication1
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
            AutofacContainer = new AutofacContainer(configuration);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Config db ConnectionString in appsettings.json
            // PM> Install-Package Microsoft.VisualStudio.Web.CodeGeneration.Design -Version 2.0.3
            // PM> Add-Migration Initial
            // PM> Update-Database
            //CMD> dotnet aspnet-codegenerator razorpage -m Movie -dc MovieContext -udl -outDir Pages\Movies --referenceScriptLibraries
            services.AddDbContext<YzDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString("YZConnectionString")));


            //Includes support for Razor Pages and controllers
            services.AddMvc();

            //Init Autofac IContainer
            AutofacContainer.ConfigureServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IApplicationLifetime lifetime, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseMvc();

            AutofacContainer.Configure(app, lifetime, env);
        }
    }
}
