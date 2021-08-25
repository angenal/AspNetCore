using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebFramework
{
    /// <summary>
    /// 数据库存储AppSettings
    /// </summary>
    public class ConfigurationDbContext : DbContext
    {
        /// <summary></summary>
        public ConfigurationDbContext(DbContextOptions options) : base(options) { }
        /// <summary></summary>
        public DbSet<AppSetting> AppSettings { get; set; }
    }
    /// <summary>
    /// 键值对Table
    /// </summary>
    public class AppSetting
    {
        /// <summary></summary>
        public string Id { get; set; }
        /// <summary></summary>
        public string Value { get; set; }
    }
    /// <summary>
    /// 数据库中的任何设置都将替代 appsettings.json 文件中的设置
    /// </summary>
    public static class ConfigurationDataSourceExtensions
    {
        /// <summary>
        /// 初始化配置.AddConfigurationDataSource(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")))
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="optionsAction"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddConfigurationDataSource(this IConfigurationBuilder builder, Action<DbContextOptionsBuilder> optionsAction)
        {
            return builder.Add(new ConfigurationDataSource(optionsAction));
        }
    }
    /// <summary></summary>
    public class ConfigurationDataSource : IConfigurationSource
    {
        /// <summary></summary>
        private readonly Action<DbContextOptionsBuilder> optionsAction;
        /// <summary></summary>
        public ConfigurationDataSource(Action<DbContextOptionsBuilder> optionsAction) => this.optionsAction = optionsAction;
        /// <summary></summary>
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new ConfigurationDataSourceProvider(optionsAction);
    }
    /// <summary></summary>
    public class ConfigurationDataSourceProvider : ConfigurationProvider
    {
        /// <summary></summary>
        public ConfigurationDataSourceProvider(Action<DbContextOptionsBuilder> optionsAction) => OptionsAction = optionsAction;

        /// <summary></summary>
        Action<DbContextOptionsBuilder> OptionsAction { get; }

        /// <summary>
        /// Loads (or reloads) the data for this provider.
        /// </summary>
        public override void Load()
        {
            var builder = new DbContextOptionsBuilder<ConfigurationDbContext>();
            OptionsAction(builder);
            using var dbContext = new ConfigurationDbContext(builder.Options);
            dbContext.Database.EnsureCreated(); // 如果没有建立db,会自动创建
            Data = !dbContext.AppSettings.Any() ? CreateAndSaveDefaultValues(dbContext) : dbContext.AppSettings.ToDictionary(c => c.Id, c => c.Value);
        }
        private static IDictionary<string, string> CreateAndSaveDefaultValues(ConfigurationDbContext dbContext)
        {
            var initialData = new Dictionary<string, string>();
            if (initialData.Count == 0) return initialData;
            dbContext.AppSettings.AddRange(initialData.Select(kv => new AppSetting { Id = kv.Key, Value = kv.Value }).ToArray());
            dbContext.SaveChanges();
            return initialData;
        }
    }
}
