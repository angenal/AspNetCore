using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebCore.Security;
using WebInterface;
using WebInterface.Settings;

namespace WebFramework.Services
{
    /// <summary>
    /// Crypto Module
    /// </summary>
    public static class CryptoModule
    {
        /// <summary>
        /// Register services
        /// </summary>
        public static IServiceCollection AddCrypto(this IServiceCollection services, IConfiguration config)
        {
            var section = config.GetSection(AesSettings.AppSettings);
            if (!section.Exists()) return services;

            // Register IOptions<AesSettings> from appsettings.json
            services.Configure<AesSettings>(section);
            config.Bind(AesSettings.AppSettings, AesSettings.Instance);

            services.AddSingleton<ICrypto>(Crypto.Instance);

            return services;
        }
    }
}
