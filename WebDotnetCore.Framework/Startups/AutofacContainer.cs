using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace WebFramework.Startups
{
    /// <summary>
    /// Autofac Container : using (var scope = Container.BeginLifetimeScope()) { var logger = scope.Resolve<ILogger>(); }
    /// </summary>
    public class AutofacContainer
    {
        /// <summary>
        /// 注册组件的Builder
        /// </summary>
        ContainerBuilder Builder { get; }
        /// <summary>
        /// 组件的Container
        /// </summary>
        public IContainer Container { get; protected set; }

        public AutofacContainer(IConfiguration configuration)
        {
            Builder = new ContainerBuilder();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Builder.RegisterModule<AutofacRegisterModule>();
            Container = Builder.Build();
            //return new AutofacServiceProvider(Container);
        }

        public void Configure(IApplicationBuilder app, IApplicationLifetime lifetime, IHostingEnvironment env)
        {
            //Application程序退出时
            lifetime.ApplicationStopped.Register(() => Container.Dispose());
        }
    }

    internal class AutofacRegisterModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //注册组件
            //Type implementationType = typeof(ILogger);
            //注册组件暴露的服务
            //builder.RegisterType(implementationType).As<ILogger>();
            //注册组件暴露组件所有服务
            //builder.RegisterType(implementationType).AsImplementedInterfaces();
            //注册组件实例暴露服务TextWriter
            //var instance = new StringWriter();
            //builder.RegisterInstance(instance).As<TextWriter>();
            //注册组件时用表达式
            //builder.Register(c => new ConfigReader("Section1")).As<IConfigReader>();
            //注册组件时用构造函数
            //builder.RegisterType<AutofacContainer>().UsingConstructor(typeof(IConfiguration));
            //注册组件时用单例并由Application控制Lifetime
            //builder.RegisterInstance(instanceSingleton).ExternallyOwned();
            //注册组件时用Application中的Repository组件并由Application控制Lifetime为每次请求
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).Where(t => t.Name.EndsWith("Repository")).AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterAssemblyTypes(GetAssemblys("Repository").ToArray()).AsImplementedInterfaces();
        }

        private IEnumerable<Assembly> GetAssemblys(string v)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            foreach (var file in dir.GetFiles(".dll"))
                if (file.Name.ToLower().Contains(v.ToLower()))
                    yield return AssemblyLoadContext.Default.LoadFromAssemblyPath(file.FullName);
        }
    }
}
