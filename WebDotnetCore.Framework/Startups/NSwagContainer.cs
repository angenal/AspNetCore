using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema;
using NSwag.AspNetCore;
using System.Reflection;

namespace WebFramework.Startups
{
    /// <summary>
    /// NSwag 使用反射，建议 Web API 返回类型为 ActionResult<T>
    /// ApiClient 代码生成 从官方 GitHub 存储库安装 https://github.com/RSuter/NSwag/wiki/NSwagStudio
    /// </summary>
    public class NSwagContainer
    {
        private readonly Assembly webApiAssembly;

        public NSwagContainer(IConfiguration configuration, Assembly webApiAssembly)
        {
            this.webApiAssembly = webApiAssembly;
        }
        public void ConfigureServices(IServiceCollection services)
        {
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // 启用静态资源访问
            //app.UseStaticFiles();

            // Register the Swagger generator
            app.UseSwagger(webApiAssembly, settings =>
            {
                settings.PostProcess = doc =>
                {
                    doc.Info.Version = "v1";
                    doc.Info.Title = "Api 版本 1.0";
                    doc.Info.Description = "A simple ASP.NET Core web API";
                    doc.Info.TermsOfService = "None";
                    doc.Info.Contact = new NSwag.SwaggerContact
                    {
                        Name = "angenal",
                        Email = "angenal@hotmail.com",
                        Url = "https://github.com/angenalZZZ/AspNetCore"
                    };
                    //doc.Info.License = new NSwag.SwaggerLicense
                    //{
                    //    Name = "angenal",
                    //    Url = "https://github.com/angenalZZZ/license"
                    //};
                };
            });

            // 导航到 http://localhost:<port>/swagger 查看 Swagger UI
            // 导航到 http://localhost:<port>/swagger/v1/swagger.json 查看 Swagger 规范
            app.UseSwaggerUi(webApiAssembly, settings =>
            {
                settings.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;
            });
        }
    }
}
