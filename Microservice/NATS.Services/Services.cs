using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.V8;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NATS.Services
{
    internal static class Services
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// <summary>Use this method to add services to the container.</summary>
        public static void Configure(HostBuilderContext context, IServiceCollection services)
        {
            // https://nuget.org/packages/JavaScriptEngineSwitcher.V8
            IJsEngineSwitcher engineSwitcher = JsEngineSwitcher.Current;
            engineSwitcher.DefaultEngineName = V8JsEngine.EngineName;
            engineSwitcher.EngineFactories.AddV8();
            IJsEngine engine = JsEngineSwitcher.Current.CreateDefaultEngine();
            services.AddSingleton(engine);
            // https://nuget.org/packages/JavaScriptEngineSwitcher.Extensions.MsDependencyInjection
            //services.AddJsEngineSwitcher(options => options.DefaultEngineName = V8JsEngine.EngineName).AddV8();

            services.AddHostedService<Worker>();
        }
    }
}
