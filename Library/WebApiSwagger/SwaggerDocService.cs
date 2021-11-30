using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using WebApiSwagger.Internals;

namespace WebApiSwagger
{
    /// <summary>
    /// Swagger服务扩展
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 注册Swagger扩展
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="enableApiVersion">启用接口版本</param>
        public static IServiceCollection AddSwaggerDoc(this IServiceCollection services, bool enableApiVersion = true)
        {
            if (enableApiVersion)
            {
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
            }
            // 启用 Swagger
            services.AddSwaggerDoc(o =>
            {
                o.ProjectName = "接口文档";
                o.RoutePrefix = "swagger";
                o.EnableApiVersion = enableApiVersion;
                o.EnableCustomIndex = true;
                o.AddSwaggerGenAction = c =>
                {
                    //config.SwaggerDoc("v1", new Info() { Title = "接口文档", Version = "v1" });

                    // 添加 XML 接口描述文档
                    foreach (string filePath in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml"))
                    {
                        if (File.Exists(filePath.Substring(0, filePath.Length - 4) + ".dll")) c.IncludeXmlComments(filePath, true);
                    }

                    c.UseInlineDefinitionsForEnums();

                    //c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>(){{"oauth2", new string[] { }}});

                    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme()
                    {
                        Name = "X-API-KEY",
                        Scheme = "oauth2",
                        Description = "API KEY",
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
                                    Id = "oauth2", Type = ReferenceType.SecurityScheme
                                },
                                Name = "apiKey", Scheme = "oauth2", In = ParameterLocation.Cookie
                            },
                            Array.Empty<string>()
                        }
                    });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                    {
                        Name = "Authorization",
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        Description = "JWT Authorization header using the Bearer scheme.",
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
                                    Id = "Bearer", Type = ReferenceType.SecurityScheme
                                },
                                Name = "token", Scheme = "Bearer", In = ParameterLocation.Header
                            },
                            Array.Empty<string>()
                        }
                    });

                    // 添加通用参数
                    c.AddCommonParameter(new List<OpenApiParameter>()
                    {
                        new OpenApiParameter()
                        {
                            Name = "X-API-VERSION",
                            In = ParameterLocation.Header,
                            Schema = new OpenApiSchema() { Type = "string", Default = new OpenApiString("v1") }
                        }
                    });

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
            // 启用 JSON.NET
            return services.AddSwaggerGenNewtonsoftSupport();
        }

        /// <summary>
        /// 注册Swagger扩展
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="setupAction">操作配置</param>
        public static IServiceCollection AddSwaggerDoc(this IServiceCollection services, Action<SwaggerDocOptions> setupAction = null)
        {
            setupAction?.Invoke(BuildContext.Instance.ExOptions);
            if (BuildContext.Instance.ExOptions.EnableCached)
            {
                services.AddSwaggerGen();
                services.AddSwaggerCaching();
                services.ConfigureSwaggerGen(o =>
                {
                    BuildContext.Instance.ExOptions.InitSwaggerGenOptions(o);
                    BuildContext.Instance.Build();
                });
                return services;
            }
            services.AddSwaggerGen(o =>
            {
                BuildContext.Instance.ExOptions.InitSwaggerGenOptions(o);
                BuildContext.Instance.Build();
            });
            return services;
        }

        /// <summary>
        /// 添加Swagger文档缓存。必须在 AddSwaggerGen() 方法之后使用。
        /// </summary>
        /// <param name="services">服务集合</param>
        public static IServiceCollection AddSwaggerCaching(this IServiceCollection services) => services.Replace(ServiceDescriptor.Transient<ISwaggerProvider, CachingSwaggerProvider>());
    }
}
