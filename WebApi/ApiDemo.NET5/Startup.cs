using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApiDemo.NET5
{
    public class Startup : WebFramework.Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment) : base(configuration, environment) { }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            ConfigApp(app, env, loggerFactory);

            //API: ApiController
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{culture:culture}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers();
                UseEndpointsMaps(endpoints);
                //endpoints.MapHub<TicketHub>("/tickethub", options => { });
            });
        }
    }
}
