using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using WebCore;
using WebInterface;
using WebSwagger;
using WebSwaggerDemo.NET5.Common;
using WebSwaggerDemo.NET5.Filters;

namespace WebSwaggerDemo.NET5
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 注册控制器
            var builder = services.AddControllers(options =>
            {
                // 全局日志记录
                //options.Filters.Add<AsyncTraceMonitorFilter>();
                // 用户会话状态 user session
                options.Filters.Add<AsyncSessionFilter>();
                // 请求参数验证 启用 FluentValidation
                //if (AsyncRequestValidationFilter.FluentValidation)
                //{
                //    options.Filters.Add<AsyncRequestValidationFilter>();
                //    options.EnableEndpointRouting = false;
                //}
            });
            // 注册用户会话
            services.AddScoped<Session>();

            // Newtonsoft.Json override the default System.Text.Json of .NET Library
            builder.AddNewtonsoftJson(x =>
            {
                x.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver(); // 驼峰命名(首字母小写)
                x.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                x.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                x.SerializerSettings.Converters.Add(new StringEnumConverter());
                //x.SerializerSettings.Converters.Add(new IsoDateTimeConverter());
                //x.SerializerSettings.Converters.Add(new JavaScriptDateTimeConverter());
                //x.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; // 不输出值为空的对象属性
            });

            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebSwaggerDemo.NET5", Version = "v1" });
            //});
            // 注册Swagger扩展
            //services.AddSwaggerDoc<Models.GroupSample>();
            services.AddSwaggerDoc<Models.GroupSample>(c => c.ProjectName = "WebSwaggerDemo.NET5");

            // Authentication with JWT
            services.AddJwtAuthentication(Configuration);
            // Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("test", policy => policy.RequireClaim("name", "测试"));
                options.AddPolicy("Upload", policy => policy.RequireAuthenticatedUser());
                options.AddPolicy("User", policy => policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "role" && c.Value.StartsWith("User")) ||
                    context.User.HasClaim(c => c.Type == "name" && c.Value.StartsWith("User"))));
            }).AddSingleton<IPermissionChecker, PermissionChecker>().AddSingleton<IPermissionStorage>(_ => new PermissionStorage(Configuration.GetConnectionString("Redis")));


            // BackgroundService: TaskService
            services.AddHostedService<TaskService>();
            // FluentScheduler: TaskManager
            services.AddSingleton<ITaskManager, TaskManager>(_ => TaskManager.Default);
            // Allow to raise a task completion source with minimal costs and attempt to avoid stalls due to thread pool starvation.
            services.AddSingleton<ITaskExecutor, TaskExecutor>(_ => TaskExecutor.Default);


            // other services
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebSwaggerDemo.NET5 v1"));
                // 启用Swagger扩展
                app.UseSwaggerDoc();
            }

            app.UseRouting();

            // Use Authentication with JWT or Microsoft.AspNetCore.Identity system
            app.UseAuthentication();
            // Use Authorization
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
