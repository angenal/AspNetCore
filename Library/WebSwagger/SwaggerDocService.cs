using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
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

                    // 添加身份验证
                    c.AddSwaggerSecurityDefinition();

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

                    // 自定义操作标识或权限ID
                    c.CustomOperationIds(apiDesc => apiDesc.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name.ToUpper() : null);

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
        /// 添加SwaggerUI
        /// </summary>
        /// <param name="options">Swagger 接口文档选项配置</param>
        /// <param name="swaggerUiOptions">Swagger 选项配置</param>
        public static void AddSwaggerUi(this SwaggerDocOptions options, SwaggerUIOptions swaggerUiOptions)
        {
            options.SwaggerUiOptions = swaggerUiOptions;
            swaggerUiOptions.RoutePrefix = options.RoutePrefix ?? "swagger";
            swaggerUiOptions.DocumentTitle = options.ProjectName ?? "REST API";
            if (options.EnableCustomIndex) swaggerUiOptions.UseCustomSwaggerIndex(); // 使用自定义首页
            if (options.EnableAuthorization())
            {
                swaggerUiOptions.ConfigObject.AdditionalItems["customAuth"] = true;
                swaggerUiOptions.ConfigObject.AdditionalItems["loginUrl"] = $"/{options.RoutePrefix}/login.html";
                swaggerUiOptions.ConfigObject.AdditionalItems["logoutUrl"] = $"/{options.RoutePrefix}/logout";
            }
            if (options.ApiVersions == null)
            {
            }
            options.UseSwaggerUIAction?.Invoke(swaggerUiOptions);
        }

        /// <summary>
        /// 初始化Swagger身份验证
        /// </summary>
        private static void InitSwaggerSecurityDefinition()
        {
            string scheme = "ApiKey";
            var scheme1 = new OpenApiSecurityScheme()
            {
                Name = "X-API-KEY",
                Description = "API 密钥",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            };
            SecurityDefinitions.Add(scheme, scheme1);

            scheme = "Bearer";
            var scheme2 = new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Scheme = scheme,
                BearerFormat = "JWT",
                Description = "JWT 认证授权",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            };
            SecurityDefinitions.Add(scheme, scheme2);
        }

        /// <summary>
        /// 添加Swagger身份验证
        /// </summary>
        /// <param name="c"></param>
        private static void AddSwaggerSecurityDefinition(this SwaggerGenOptions c)
        {
            //c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>(){{"oauth2", new string[] { }}});

            string scheme = "ApiKey", queryName = "apiKey";
            c.AddSecurityDefinition(scheme, SecurityDefinitions[scheme]);
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
            c.AddSecurityDefinition(scheme, SecurityDefinitions[scheme]);
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
        }

        /// <summary>
        /// Swagger使用localStorage存储Token
        /// </summary>
        /// <param name="c"></param>
        private static void UseSwaggerSecurityStorage(this SwaggerUIOptions c)
        {
            InitSwaggerSecurityDefinition();
            string scheme = "ApiKey";
            c.UseTokenStorage(scheme, SecurityDefinitions[scheme].ConvertToTDP(), WebCacheType.Local);
            scheme = "Bearer";
            c.UseTokenStorage(scheme, SecurityDefinitions[scheme].ConvertToTDP(), WebCacheType.Session);
        }

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

                    // 使用localStorage存储Token
                    c.UseSwaggerSecurityStorage();

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
            return app.UseSwagger(o => BuildContext.Instance.DocOptions.InitSwaggerOptions(o)).UseSwaggerUI(o => BuildContext.Instance.DocOptions.AddSwaggerUi(o));
        }

        public static readonly Dictionary<string, OpenApiSecurityScheme> SecurityDefinitions = new Dictionary<string, OpenApiSecurityScheme>();
        public static Assembly Assembly => typeof(SwaggerDocService).GetTypeInfo().Assembly;
    }
}
