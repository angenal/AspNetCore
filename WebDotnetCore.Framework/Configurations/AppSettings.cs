using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WebFramework.Configurations
{
    public class AppSettings
    {
        public IConfiguration Configuration { get; set; }
        public IHostingEnvironment Environment { get; set; }

        public AppSettings(IHostingEnvironment environment, IConfiguration configuration = null,
            Dictionary<string, string> initialData = null,
            string path = "appsettings.json", bool reloadOnChange = true)
        {
            Environment = environment;
            Configuration = configuration == null
                ? FromConfigurationBuilder(environment, initialData, path, reloadOnChange).Build()
                : configuration;
        }

        /// <summary>
        /// 读取配置 初始化配置
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="initialData"></param>
        /// <param name="path"></param>
        /// <param name="reloadOnChange"></param>
        /// <returns></returns>
        public static IConfigurationBuilder FromConfigurationBuilder(IHostingEnvironment environment,
            Dictionary<string, string> initialData = null,
            string path = "appsettings.json", bool optional = true, bool reloadOnChange = true)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile(path, optional, reloadOnChange)
                .AddJsonFile(path.Replace(".json", environment.EnvironmentName + ".json"), optional, reloadOnChange)
                .AddEnvironmentVariables();

            if (initialData != null && initialData.Count > 0)
            {
                builder.AddInMemoryCollection(initialData);
            }

            return builder;
        }

        /// <summary>
        /// DI 配置
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServicesAddOptions(IServiceCollection services)
        {
            // Setup options with DI
            services.AddOptions();

            // Configure ConnectionStrings using config
            services.Configure<ConnectionStrings>(Configuration);
        }
        /// <summary>
        /// 获取配置 DI 测试
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public ConnectionStrings InjectOptions(IOptions<ConnectionStrings> options)
        {
            return options.Value;
        }
        /// <summary>
        /// 获取配置 单个值（配置 API 交互时，冒号(分层配置值:)适用于所有平台）如:"Logging:LogLevel:Default"
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                return Configuration[key];
            }
            set
            {
                Configuration[key] = value;
            }
        }
        /// <summary>
        /// 获取配置 单个值 如:GetValue<string>("Administrators:0:Name")-取数组元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetValue<T>(string key)
        {
            return Configuration.GetValue<T>(key);
        }
        /// <summary>
        /// 获取配置 绑定到对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Configuration.Get<T>();
            }
            return Configuration.GetSection(key).Get<T>();
            //Configuration.GetSection(key).Bind(instance);
        }
    }

    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; }
    }
}
