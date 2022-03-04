using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace WebFramework.Data
{
    /// <summary>
    /// RavenDB database service.
    /// </summary>
    public class RavenDb
    {
        internal static X509Certificate2 cert;
        // <PackageReference Include="Serilog.Sinks.RavenDB" Version="3.0.0" />

        /// <summary></summary>
        public static IDocumentStore CreateRavenDocStore(IWebHostEnvironment env,
            string connectionString = "https://a.free.xxx.ravendb.cloud",
            string database = "app", string certPassword = "UsawpF4uIKy5Ju7P",
            string databaseDevelopment = "test", string certDevelopmentPassword = "UsawpF4uIKy5Ju7P")
        {
            // <PackageReference Include="Serilog.Sinks.RavenDB" Version="3.0.0" />
            RequestExecutor.RemoteCertificateValidationCallback += RemoteCertificateValidationCallback;

            var db = new DocumentStore { Urls = new[] { connectionString } };
            var dir = Directory.GetCurrentDirectory();
            cert = !string.IsNullOrEmpty(databaseDevelopment) && env.IsDevelopment()
                ? new X509Certificate2($"{dir}/{databaseDevelopment}.pfx", certDevelopmentPassword)
                : new X509Certificate2($"{dir}/{database}.pfx", certPassword);
            db.Certificate = cert;
            db.Database = database;
            db.Initialize();

            return db;
        }

        /// <summary></summary>
        public static bool RemoteCertificateValidationCallback(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors) => true;
    }
}
