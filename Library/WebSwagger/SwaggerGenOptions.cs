using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebSwagger.Internals;

namespace WebSwagger
{
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
        public SwaggerGenConfigureOptions(IApiVersionDescriptionProvider provider)
        {
            this.provider = provider;
        }

        /// <inheritdoc />
        public void Configure(SwaggerGenOptions options)
        {
            bool hasSwaggerDoc = false;
            SwaggerDocOptions docs = BuildContext.Instance.DocOptions;
            OpenApiContact contact = null; // new OpenApiContact { Name = "developer", Email = "developer@company.com", Url = new Uri("https://www.company.com/") };
            OpenApiLicense license = null; // new OpenApiLicense { Name = "MIT License", Url = new Uri("https://opensource.org/licenses/MIT") };
            if (docs.EnableApiVersion)
            {
                // add a swagger document for each discovered API version
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    var info = new OpenApiInfo()
                    {
                        Title = docs.ProjectName,
                        Description = docs.ProjectDescription,
                        Version = description.ApiVersion.ToString(),
                        Contact = contact,
                        License = license,
                    };

                    if (description.IsDeprecated)
                    {
                        info.Description += " This API version has been deprecated.";
                    }

                    options.SwaggerDoc(description.GroupName, info);
                    hasSwaggerDoc = true;
                }
            }
            if (!hasSwaggerDoc)
            {
                var v1Info = new OpenApiInfo
                {
                    Title = docs.ProjectName,
                    Description = docs.ProjectDescription,
                    Version = "1.0",
                    Contact = contact,
                    License = license,
                };
                options.SwaggerDoc("v1", v1Info);
            }
        }
    }
}
