using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using WebSwagger.Core.Authorization;
using WebSwagger.Internals;

namespace WebSwagger
{
    /// <summary>
    /// Swagger服务扩展
    /// </summary>
    public static partial class SwaggerDocServices
    {
        /// <summary>
        /// Swagger 接口文档选项配置(默认)
        /// </summary>
        /// <param name="options"></param>
        public static void AddDefaltDocOptions(SwaggerDocOptions options, Type apiGroupType)
        {
            options.ProjectName = "API";
            options.RoutePrefix = "swagger";
            options.ApiGroupType = apiGroupType;
            options.EnableApiVersion = apiGroupType == null;
            options.EnableCustomIndex = true;
            options.AddSwaggerGenAction = c =>
            {
                // 添加 XML 接口描述文档
                foreach (string filePath in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml"))
                {
                    if (File.Exists(filePath.Substring(0, filePath.Length - 4) + ".dll")) c.IncludeXmlComments(filePath, true);
                }

                c.IgnoreObsoleteActions();
                c.UseInlineDefinitionsForEnums();

                // 显示枚举描述
                c.ShowEnumDescription();

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
                if (options.EnableApiVersion) c.OperationFilter<Filters.Operations.ApiVersionDefaultValueOperationFilter>();

                // 上传文件 显示文件参数
                c.ShowFileParameter();
                c.MapType<IFormFile>(() => new OpenApiSchema() { Type = "file" });

                // 控制器排序
                c.OrderByController();

                // 显示Url模式：首字母小写、首字母大写、全小写、全大写、默认
                c.ShowUrlMode();

                // 隐藏属性
                c.SchemaFilter<Filters.Schemas.IgnorePropertySchemaFilter>();

                // 自定义操作Id 导航 Controller's Action
                c.CustomOperationIds(apiDesc =>
                {
                    var name = apiDesc.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name : null;
                    var attributes = apiDesc.CustomAttributes();
                    if (attributes.Any(t => t is AllowAnonymousAttribute))
                    {
                        return $"匿名访问{name}";
                    }
                    var attribute = attributes.FirstOrDefault(t => t is AuthorizeAttribute);
                    if (attribute != null)
                    {
                        var s = new StringBuilder();
                        if (attribute is AllowUserAttribute)
                        {
                            var operationAttribute = (AllowUserAttribute)attribute;
                            if (!string.IsNullOrEmpty(operationAttribute.Roles))
                                s.AppendFormat("角色{0}", operationAttribute.Roles);
                            if (!string.IsNullOrEmpty(operationAttribute.Permissions))
                                s.AppendFormat("权限{0}", operationAttribute.Permissions);
                            if (!string.IsNullOrEmpty(operationAttribute.Policy))
                                s.AppendFormat("策略{0}", operationAttribute.Policy);
                        }
                        else
                        {
                            var authorizeAttribute = (AuthorizeAttribute)attribute;
                            if (!string.IsNullOrEmpty(authorizeAttribute.Roles))
                                s.AppendFormat("角色{0}", authorizeAttribute.Roles);
                            if (!string.IsNullOrEmpty(authorizeAttribute.Policy))
                                s.AppendFormat("策略{0}", authorizeAttribute.Policy);
                        }
                        return s.Length == 0 ? $"授权访问{name}" : $"授权访问{name}{s}";
                    }
                    return name;
                });

                // 显示授权信息
                //c.ShowAuthorizeInfo();
            };
        }

        /// <summary>
        /// 注册Swagger
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="setupAction">操作配置</param>
        public static IServiceCollection AddSwaggerDoc<T>(this IServiceCollection services, Action<SwaggerDocOptions> setupAction = null)
        {
            // Setup the Swagger generation options.
            AddDefaltDocOptions(BuildContext.Instance.DocOptions, typeof(T));
            setupAction?.Invoke(BuildContext.Instance.DocOptions);
            if (BuildContext.Instance.DocOptions.EnableApiVersion)
            {
                services.AddApiVersioning(o =>
                {
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader(BuildContext.Instance.DocOptions.QueryApiVersion), new UrlSegmentApiVersionReader());
                    //options.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader(BuildContext.Instance.DocOptions.QueryApiVersion), new UrlSegmentApiVersionReader(), new HeaderApiVersionReader() { HeaderNames = { BuildContext.Instance.DocOptions.HeaderApiVersion } });
                    o.ReportApiVersions = false; // headers "api-supported-versions" and "api-deprecated-versions"
                    o.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                    o.RouteConstraintName = BuildContext.Instance.DocOptions.QueryApiVersion;
                });
                services.AddVersionedApiExplorer(o =>
                {
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    o.GroupNameFormat = "'v'VVV";
                    //o.GroupNameFormat = "'v'VVVV";
                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    o.SubstituteApiVersionInUrl = true;
                });
            }
            // Setup JSON.NET
            services.AddSwaggerGenNewtonsoftSupport();
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
            swaggerUiOptions.RoutePrefix = !string.IsNullOrEmpty(options.RoutePrefix) ? options.RoutePrefix : "swagger";
            swaggerUiOptions.DocumentTitle = !string.IsNullOrEmpty(options.ProjectName) ? options.ProjectName : "API";
            if (options.EnableCustomIndex) swaggerUiOptions.UseCustomSwaggerIndex();
            if (options.EnableAuthorization())
            {
                swaggerUiOptions.ConfigObject.AdditionalItems["customAuth"] = true;
                swaggerUiOptions.ConfigObject.AdditionalItems["loginUrl"] = $"/{options.RoutePrefix}/login.html";
                swaggerUiOptions.ConfigObject.AdditionalItems["logoutUrl"] = $"/{options.RoutePrefix}/logout";
            }
            options.UseSwaggerUIAction?.Invoke(swaggerUiOptions);
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
                    c.DocumentTitle = !string.IsNullOrEmpty(c.DocumentTitle) ? c.DocumentTitle : "API";
                    c.ConfigObject.SupportedSubmitMethods = new SubmitMethod[] { SubmitMethod.Get, SubmitMethod.Put, SubmitMethod.Post, SubmitMethod.Delete, SubmitMethod.Options };
                    c.ConfigObject.ShowCommonExtensions = true;
                    c.ConfigObject.ValidatorUrl = null;

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
        public static Assembly Assembly => typeof(SwaggerDocServices).GetTypeInfo().Assembly;
    }
}
