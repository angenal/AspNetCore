using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebInterface.Settings;

namespace WebFramework.Services
{
    /// <summary>
    /// Identity Module: Identity system for the specified User and Role types.
    /// </summary>
    public static class IdentityModule
    {
        /// <summary>
        /// Register services
        /// </summary>
        public static IServiceCollection AddIdentityLiteDB(this IServiceCollection services, IConfiguration config)
        {
            var section = config.GetSection(IdentitySettings.AppSettings);
            if (!section.Exists()) return services;

            // Register IOptions<IdentitySettings> from appsettings.json
            services.Configure<IdentitySettings>(section);
            config.Bind(IdentitySettings.AppSettings, IdentitySettings.Instance);

            services.AddIdentity<Identity.LiteDB.Models.ApplicationUser, Identity.LiteDB.IdentityRole>(options =>
            {
                options.User = IdentitySettings.Instance.User;
                options.SignIn = IdentitySettings.Instance.SignIn;
                options.Password = IdentitySettings.Instance.Password;
            })
            //.AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserStore<Identity.LiteDB.LiteDbUserStore<Identity.LiteDB.Models.ApplicationUser>>()
            .AddRoleStore<Identity.LiteDB.LiteDbRoleStore<Identity.LiteDB.IdentityRole>>()
            .AddDefaultTokenProviders();

            return services;
        }
    }
}
