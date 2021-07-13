using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reflection;
using WebCore.Security;

namespace WebFramework.Services
{
    /// <summary>
    /// AutofacStartup extends Startup class
    /// </summary>
    public class AutofacStartup
    {
        /// <summary>
        /// Configure default container in Startup class
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder) => builder.RegisterModule(new AutofacServicesModule());
        /// <summary>
        /// Constructor in Startup class
        /// </summary>
        public AutofacStartup(IConfiguration configuration, IWebHostEnvironment environment) { }
    }

    /// <summary>
    /// Autofac for default container service provider
    /// </summary>
    public static class AutofacProviderConfigure
    {
        /// <summary>
        /// Configure default container service provider in Program:Main:CreateHostBuilder
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureAutofac(this IHostBuilder builder) => builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    }

    /// <summary>
    /// Autofac services module
    /// </summary>
    public class AutofacServicesModule : Autofac.Module
    {
        static readonly string[] Types = new[] { "Controller", "Service" };

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // Register assembly types
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AsImplementedInterfaces().Where(i => Types.Any(t => i.Name.EndsWith(t)))
                .PropertiesAutowired().AsSelf().InstancePerDependency();

            // Register custom types
            builder.RegisterType<Crypto>().AsSelf().SingleInstance();
        }
    }
}
