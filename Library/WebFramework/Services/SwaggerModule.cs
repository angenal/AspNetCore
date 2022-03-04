using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WebInterface.Settings;

namespace WebFramework.Services
{
    /// <summary>
    /// Swagger Doc service.
    /// https://docs.microsoft.com/zh-cn/aspnet/core/tutorials/getting-started-with-swashbuckle
    /// </summary>
    public static class SwaggerDocService
    {
        /// <summary>
        /// HttpRequest Query Variable for JwtAuthenticationService
        /// </summary>
        internal const string apiKeyQueryOrCookieParameterName = ApiSettings.HttpRequestQuery;

        /// <summary>
        /// Register Swagger Doc service.
        /// http://localhost:5000/swagger/index.html
        /// http://localhost:5000/swagger/v1/swagger.json
        /// </summary>
        public static IServiceCollection AddSwaggerGen(this IServiceCollection services, IConfiguration config)
        {
            var section = config.GetSection(ApiSettings.AppSettings);
            if (!section.Exists()) return services;

            // Configures ApiSettings
            if (ApiSettings.Instance == null)
            {
                ApiSettings.Instance = new ApiSettings();
                // Register IOptions<ApiSettings> from appsettings.json
                services.Configure<ApiSettings>(section);
                config.Bind(ApiSettings.AppSettings, ApiSettings.Instance);
            }

            // Configures the Swagger generation options.
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerGenConfigureOptions>();

            // Adds the Swagger Generator.
            services.AddSwaggerGen(c =>
            {
                // add operation filter which sets default values.
                if (ApiVersionService.UseVersionedApiExplorer) c.OperationFilter<SwaggerDefaultValues>();
                // integrate xml comments, set project properties to generate XML file.
                foreach (string filePath in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml"))
                {
                    if (File.Exists(filePath.Substring(0, filePath.Length - 4) + ".dll")) c.IncludeXmlComments(filePath, true);
                }
                // add authentication security scheme.
                string name = "Authorization", scheme = JwtBearerDefaults.AuthenticationScheme;
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = name,
                    Scheme = scheme,
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                };
                c.AddSecurityDefinition(scheme, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = scheme, Type = ReferenceType.SecurityScheme
                            },
                            Name = apiKeyQueryOrCookieParameterName, Scheme = scheme, In = ParameterLocation.Header
                        },
                        Array.Empty<string>()
                    }
                });
                // provide a custom strategy for generating the unique Id's that are used to reference object Schema's
                c.CustomSchemaIds(i => i.FullName);
            });

            // Adds the Swagger Generator opt-in component to support Newtonsoft.Json serializer behaviors.
            services.AddSwaggerGenNewtonsoftSupport();

            return services;
        }
        /// <summary>
        /// Use Swagger Doc Generator.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSwaggerGen(this IApplicationBuilder app, IConfiguration config)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                if (ApiVersionService.UseVersionedApiExplorer)
                {
                    // build a swagger endpoint for each discovered API version
                    var provider = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", ApiVersionService.SelectDefinitionPrefix + description.GroupName);
                    }
                    return;
                }

                var versions = new List<string> { ApiVersionService.DefaultVersion };
                versions.AddRange(ApiVersionService.Versions.Keys.Where(version => !version.Equals(ApiVersionService.DefaultVersion)).OrderBy(version => version));
                foreach (string version in versions) options.SwaggerEndpoint($"/swagger/{version}/swagger.json", ApiVersionService.Versions[version].Name);
                //options.RoutePrefix = string.Empty; // default: "swagger"
            });
            //app.UseWelcomePage("/swagger");
            return app;
        }
    }

    /// <summary>
    /// Configures the Swagger generation options.
    /// </summary>
    /// <remarks>This allows API versioning to define a Swagger document per API version after the
    /// <see cref="IApiVersionDescriptionProvider"/> service has been resolved from the service container.</remarks>
    public class SwaggerGenConfigureOptions : IConfigureOptions<SwaggerGenOptions>
    {
        readonly IApiVersionDescriptionProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerGenConfigureOptions"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.</param>
        public SwaggerGenConfigureOptions(IApiVersionDescriptionProvider provider) => this.provider = provider;

        /// <inheritdoc />
        public void Configure(SwaggerGenOptions options)
        {
            OpenApiContact contact = null; // new OpenApiContact { Name = "developer", Email = "developer@company.com", Url = new Uri("https://www.company.com/") };
            OpenApiLicense license = null; // new OpenApiLicense { Name = "MIT License", Url = new Uri("https://opensource.org/licenses/MIT") };

            if (ApiVersionService.UseVersionedApiExplorer)
            {
                // add a swagger document for each discovered API version
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    var info = new OpenApiInfo()
                    {
                        Title = ApiSettings.Instance.Title,
                        Description = ApiSettings.Instance.Description,
                        Version = description.ApiVersion.ToString(),
                        Contact = contact,
                        License = license,
                    };

                    if (description.IsDeprecated)
                    {
                        info.Description += " This API version has been deprecated.";
                    }

                    options.SwaggerDoc(description.GroupName, info);
                }
                return;
            }

            foreach (string version in ApiVersionService.Versions.Keys)
            {
                var ver = ApiVersionService.Versions[version];
                var info = new OpenApiInfo
                {
                    Title = ApiSettings.Instance.Title ?? ver?.Name,
                    Description = ApiSettings.Instance.Description ?? ver?.Description,
                    Version = string.Join(", ", ver.Versions.Select(v => v.ToString()).OrderBy(v => v)),
                    Contact = contact,
                    License = license,
                };
                options.SwaggerDoc(version, info);
            }
        }
    }

    /// <summary>
    /// Represents the Swagger/Swashbuckle operation filter used to document the implicit API version parameter.
    /// </summary>
    /// <remarks>This <see cref="IOperationFilter"/> is only required due to bugs in the <see cref="SwaggerGenerator"/>.
    /// Once they are fixed and published, this class can be removed.</remarks>
    public class SwaggerDefaultValues : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;

            operation.Deprecated |= apiDescription.IsDeprecated();

            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
            foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
            {
                // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/b7cf75e7905050305b115dd96640ddd6e74c7ac9/src/Swashbuckle.AspNetCore.SwaggerGen/SwaggerGenerator/SwaggerGenerator.cs#L383-L387
                var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
                var response = operation.Responses[responseKey];

                foreach (var contentType in response.Content.Keys)
                {
                    if (!responseType.ApiResponseFormats.Any(x => x.MediaType == contentType))
                    {
                        response.Content.Remove(contentType);
                    }
                }
            }

            if (operation.Parameters == null)
            {
                return;
            }

            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                if (parameter.Schema.Default == null && description.DefaultValue != null)
                {
                    // REF: https://github.com/Microsoft/aspnet-api-versioning/issues/429#issuecomment-605402330
                    var json = JsonSerializer.Serialize(description.DefaultValue, description.ModelMetadata.ModelType);
                    parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(json);
                }

                parameter.Required |= description.IsRequired;
            }
        }
    }
}
