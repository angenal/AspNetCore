using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using WebFramework.Filters;

namespace WebFramework.Services
{
    /// <summary>
    /// Configure Global Input Validation module
    /// </summary>
    public static class ValidationModule
    {
        /// <summary>
        /// Init Exception Handler services
        /// </summary>
        public static void AddValidation(this IServiceCollection services, IMvcBuilder builder)
        {
            // Global Error Handler using FluentValidation for Status 400 BadRequest
            if (AsyncRequestValidationFilter.FluentValidation)
            {
                builder.ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);
                services.AddFluentValidation(c =>
                {
                    //c.LocalizationEnabled = true;
                    //c.DisableDataAnnotationsValidation = true; // 关闭系统自带模型验证(否则先进行系统自带模型验证,再进行第三方库FluentValidation)
                    //c.RunDefaultMvcValidationAfterFluentValidationExecutes = false;

                    //var assembly = Assembly.GetEntryAssembly();
                    //var assemblies = new List<Assembly> { assembly };
                    //assemblies.AddRange(assembly.GetReferencedAssemblies().Select(Assembly.Load).Where(a => a.ExportedTypes.Any(t => t.GetCustomAttribute<ApiControllerAttribute>() != null)));
                    c.RegisterValidatorsFromAssemblies(Startup.Assemblies);
                });
                return;
            }
            // Global Error Handler for Status 400 BadRequest with Invalid ModelState
            builder.ConfigureApiBehaviorOptions(BadRequestHandler);
        }

        /// <summary>
        /// Global Error Handler for Status 400 BadRequest with Invalid ModelState
        /// </summary>
        public static void BadRequestHandler(ApiBehaviorOptions options)
        {
            options.InvalidModelStateResponseFactory = AsyncRequestValidationFilter.BadRequestResponse;
            //options.ClientErrorMapping[StatusCodes.Status404NotFound].Link = "https://*.com/404";
            //options.SuppressConsumesConstraintForFormFileParameters = true;
            //options.SuppressInferBindingSourcesForParameters = true;
            //options.SuppressModelStateInvalidFilter = true; // 关闭系统自带模型验证(使用第三方库FluentValidation)
            //options.SuppressMapClientErrors = true;
        }
    }
}
