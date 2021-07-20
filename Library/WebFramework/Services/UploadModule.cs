using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio.AspNetCore;

namespace WebFramework.Services
{
    /// <summary>
    /// Upload Module
    /// </summary>
    public static class UploadModule
    {
        static MinioOptions MinioOptions = new MinioOptions();
        const string MinioAppSettings = "Upload:Minio";
        /*
          "Upload": {
            "Minio": {
              "Endpoint": "play.min.io",
              "AccessKey": "Q3AM3UQ867SPQQA43P2F",
              "SecretKey": "zuf+tfteSlswRu7BJ86wekitnifILbZam1KYY3TG",
              "Region": "us-east-1",
              "SessionToken": ""
            }
          }
        */

        /// <summary>
        /// Register services
        /// </summary>
        public static IServiceCollection AddUpload(this IServiceCollection services, IConfiguration config)
        {
            // Minio  https://min.io/download
            // Minio Docs  https://docs.min.io/docs/dotnet-client-quickstart-guide.html
            // Minio.AspNetCore  https://github.com/appany/Minio.AspNetCore
            // > dotnet add package Minio.AspNetCore

            var section = config.GetSection(MinioAppSettings);
            if (!section.Exists()) return services;

            // Register IOptions<MinioOptions> from appsettings.json
            services.Configure<MinioOptions>(section);
            config.Bind(MinioAppSettings, MinioOptions);

            services.AddMinio(options =>
            {
                options.Endpoint = MinioOptions.Endpoint;
                options.AccessKey = MinioOptions.AccessKey;
                options.SecretKey = MinioOptions.SecretKey;
                options.Region = MinioOptions.Region;
                options.SessionToken = MinioOptions.SessionToken;
                options.OnClientConfiguration = client =>
                {
                    client.SetTraceOff(); // Sets HTTP tracing Off.
                    client.WithSSL(); // Connects to Cloud Storage with HTTPS if this method is invoked on client object.
                    client.WithTimeout(3600 * 1000); // Timeout in milliseconds.
                };
            });

            // Get or inject first minio client
            //var client = serviceProvider.GetRequiredService<MinioClient>();

            return services;
        }
    }
}
