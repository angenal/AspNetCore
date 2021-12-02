using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Reflection;
using WebSwagger.Core.Authorization;
using WebSwagger.Internals;

namespace WebSwagger
{
    /// <summary>
    /// Swagger服务扩展
    /// </summary>
    public static partial class SwaggerDocService
    {
        /// <summary>
        /// 注册Swagger
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="enableApiVersion">启用接口版本</param>
        public static IServiceCollection AddSwaggerDoc(this IServiceCollection services)
        {
            // 启用 Swagger
            services.AddSwaggerDoc(o =>
            {
                o.ProjectName = "接口文档";
                o.RoutePrefix = "swagger";
                o.EnableApiVersion = true;
                o.EnableCustomIndex = true;
                o.AddSwaggerGenAction = c =>
                {
                    //c.SwaggerDoc("v1", new Info() { Title = "接口文档", Version = "v1" });

                    // 添加 XML 接口描述文档
                    foreach (string filePath in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml"))
                    {
                        if (File.Exists(filePath.Substring(0, filePath.Length - 4) + ".dll")) c.IncludeXmlComments(filePath, true);
                    }

                    c.UseInlineDefinitionsForEnums();

                    // 添加 身份验证 安全方案
                    //c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>(){{"oauth2", new string[] { }}});
                    string scheme = SecuritySchemeType.ApiKey.ToString(), queryName = "apiKey";
                    c.AddSecurityDefinition(scheme, new OpenApiSecurityScheme()
                    {
                        Name = "X-API-KEY",
                        Description = "API 密钥",
                        In = ParameterLocation.Cookie,
                        Type = SecuritySchemeType.ApiKey
                    });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = scheme, Type = ReferenceType.SecurityScheme
                                },
                                Name = queryName, In = ParameterLocation.Query
                            },
                            Array.Empty<string>()
                        }
                    });
                    scheme = "Bearer"; queryName = "token";
                    c.AddSecurityDefinition(scheme, new OpenApiSecurityScheme()
                    {
                        Name = "Authorization",
                        Scheme = scheme,
                        BearerFormat = "JWT",
                        Description = "JWT 认证授权",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey
                    });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = scheme, Type = ReferenceType.SecurityScheme
                                },
                                Name = queryName, Scheme = scheme, In = ParameterLocation.Header
                            },
                            Array.Empty<string>()
                        }
                    });

                    // 添加通用参数
                    //c.AddCommonParameter(new List<OpenApiParameter>()
                    //{
                    //    new OpenApiParameter()
                    //    {
                    //        Name = "X-API-VERSION",
                    //        In = ParameterLocation.Header,
                    //        Schema = new OpenApiSchema() { Type = "string", Default = new OpenApiString("v1") }
                    //    }
                    //});

                    // 启用请求头过滤器。显示Swagger自定义请求头
                    c.EnableRequestHeader();

                    // 启用响应由过滤器。显示Swagger自定义响应头
                    c.EnableResponseHeader();

                    // 启用默认值
                    c.EnableDefaultValue();
                    if (o.EnableApiVersion) c.OperationFilter<Filters.Operations.ApiVersionDefaultValueOperationFilter>();

                    // 上传文件 显示文件参数
                    c.ShowFileParameter();
                    c.MapType<IFormFile>(() => new OpenApiSchema() { Type = "file" });

                    // 显示枚举描述
                    c.ShowEnumDescription();

                    // 控制器排序
                    c.OrderByController();

                    // 显示Url模式：首字母小写、首字母大写、全小写、全大写、默认
                    c.ShowUrlMode();

                    // 隐藏属性
                    c.SchemaFilter<Filters.Schemas.IgnorePropertySchemaFilter>();

                    // 自定义操作标识
                    c.CustomOperationIds(apiDesc => apiDesc.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name : null);

                    // 显示授权信息
                    //config.ShowAuthorizeInfo();
                };
            });
            // 启用 Version
            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = false;
            });
            services.AddVersionedApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVVV";
                o.SubstituteApiVersionInUrl = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
            });
            // 启用 JSON.NET
            return services.AddSwaggerGenNewtonsoftSupport();
        }

        /// <summary>
        /// 注册Swagger
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="setupAction">操作配置</param>
        public static IServiceCollection AddSwaggerDoc(this IServiceCollection services, Action<SwaggerDocOptions> setupAction)
        {
            setupAction?.Invoke(BuildContext.Instance.DocOptions);
            if (BuildContext.Instance.DocOptions.EnableCached)
            {
                services.AddSwaggerGen();
                services.AddSwaggerCaching();
                services.ConfigureSwaggerGen(o =>
                {
                    BuildContext.Instance.DocOptions.InitSwaggerGenOptions(o);
                    BuildContext.Instance.Build();
                });
                return services;
            }
            services.AddSwaggerGen(o =>
            {
                BuildContext.Instance.DocOptions.InitSwaggerGenOptions(o);
                BuildContext.Instance.Build();
            });
            return services;
        }

        /// <summary>
        /// 添加Swagger文档缓存。必须在 AddSwaggerGen() 方法之后使用。
        /// </summary>
        /// <param name="services">服务集合</param>
        public static IServiceCollection AddSwaggerCaching(this IServiceCollection services) => services.Replace(ServiceDescriptor.Transient<ISwaggerProvider, CachingSwaggerProvider>());

        /// <summary>
        /// 启用Swagger
        /// </summary>
        /// <param name="app">应用构建器</param>
        /// <param name="setupAction">配置操作</param>
        public static IApplicationBuilder UseSwaggerDoc(this IApplicationBuilder app)
        {
            return app.UseSwaggerDoc(o =>
            {
                o.UseSwaggerAction = c =>
                {
                    c.SerializeAsV2 = true;
                };
                o.UseSwaggerUIAction = c =>
                {
                    //config.SwaggerEndpoint("/swagger/v1/swagger.json", "接口文档 v1");

                    // 使用内部资源
                    c.UseInternalResources();

                    // 使用默认SwaggerUI
                    c.UseDefaultSwaggerUI();

                    // 使用自定义首页
                    c.UseCustomSwaggerIndex();

                    // 启用Token存储localStorage
                    c.UseTokenStorage(SecuritySchemeType.ApiKey.ToString(), WebCacheType.Local);

                    // 使用翻译
                    c.UseTranslate();
                };
            });
        }

        /// <summary>
        /// 启用Swagger
        /// </summary>
        /// <param name="app">应用构建器</param>
        /// <param name="setupAction">配置操作</param>
        public static IApplicationBuilder UseSwaggerDoc(this IApplicationBuilder app, Action<SwaggerDocOptions> setupAction = null)
        {
            BuildContext.Instance.ServiceProvider = app.ApplicationServices;
            setupAction?.Invoke(BuildContext.Instance.DocOptions);
            // 启用 Swagger 授权中间件
            if (BuildContext.Instance.DocOptions.EnableAuthorization()) app.UseMiddleware<SwaggerAuthorizeMiddleware>();
            // 启用 Swagger UI
            app.UseSwagger(o => BuildContext.Instance.DocOptions.InitSwaggerOptions(o)).UseSwaggerUI(o => BuildContext.Instance.DocOptions.InitSwaggerUi(o));
            return app;
        }

        /// <summary>
        /// 初始化SwaggerUI
        /// </summary>
        /// <param name="options">Swagger 接口文档选项配置</param>
        /// <param name="swaggerUiOptions">Swagger 选项配置</param>
        public static void InitSwaggerUi(this SwaggerDocOptions options, SwaggerUIOptions swaggerUiOptions)
        {
            options.SwaggerUiOptions = swaggerUiOptions;
            swaggerUiOptions.RoutePrefix = options.RoutePrefix;
            swaggerUiOptions.DocumentTitle = options.ProjectName;
            if (options.EnableCustomIndex) swaggerUiOptions.UseCustomSwaggerIndex();
            if (options.EnableAuthorization())
            {
                swaggerUiOptions.ConfigObject.AdditionalItems["customAuth"] = true;
                swaggerUiOptions.ConfigObject.AdditionalItems["loginUrl"] = $"/{options.RoutePrefix}/login.html";
                swaggerUiOptions.ConfigObject.AdditionalItems["logoutUrl"] = $"/{options.RoutePrefix}/logout";
            }
            if (options.ApiVersions == null)
            {
                options.UseSwaggerUIAction?.Invoke(swaggerUiOptions);
                return;
            }
            options.UseSwaggerUIAction?.Invoke(swaggerUiOptions);
        }

        public static Assembly Assembly => typeof(SwaggerDocService).GetTypeInfo().Assembly;
    }
}
