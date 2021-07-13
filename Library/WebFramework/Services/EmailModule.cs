using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebCore;
using WebInterface;
using WebInterface.Settings;

namespace WebFramework.Services
{
    /// <summary>
    /// Email Module
    /// </summary>
    public static class EmailModule
    {
        /// <summary>
        /// Register services
        /// </summary>
        public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration config)
        {
            var section = config.GetSection(SmtpSettings.AppSettings);
            if (!section.Exists()) return services;

            // Register IOptions<SmtpSettings> from appsettings.json
            services.Configure<SmtpSettings>(section);
            config.Bind(SmtpSettings.AppSettings, SmtpSettings.Instance);

            services.AddTransient<IEmailTools, EmailTools>();

            return services;
        }
    }
}
