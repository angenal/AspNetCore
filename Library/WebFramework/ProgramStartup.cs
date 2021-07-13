using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebFramework.Services;

namespace WebFramework
{
    class ProgramStartup
    {
        readonly IConfiguration Configuration;
        readonly IWebHostEnvironment Environment;

        public ProgramStartup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Adds services for api controllers
            var builder = services.AddApiControllers(); // services.AddMvc() for Mvc application

            // Services: Crypto,Database,Compression,Caching,CORS,Authentication,Authorization,Swagger,i18n...
            services.ConfigureServices(Configuration, Environment, builder);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            // Services: Crypto,Database,Compression,Caching,CORS,Authentication,Authorization,Swagger,i18n...
            app.Configure(Configuration, env, loggerFactory);

            //app.UseAbp();
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("/", async context =>
            //    {
            //        await context.Response.WriteAsync("Hello World!");
            //    });
            //});

            //API: ApiController
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{culture:culture}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers();
                //endpoints.MapHub<TicketHub>("/hub", options => { }); // SignalR
            });
        }
    }
}
