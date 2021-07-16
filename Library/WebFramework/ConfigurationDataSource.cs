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
        public ConfigurationDbContext(DbContextOptions options) : base(options) { }
        public DbSet<AppSetting> AppSettings { get; set; }
    }
    /// <summary>
    /// 键值对Table
    /// </summary>
    public class AppSetting
    {
        public string Id { get; set; }
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
    public class ConfigurationDataSource : IConfigurationSource
    {
        private readonly Action<DbContextOptionsBuilder> optionsAction;
        public ConfigurationDataSource(Action<DbContextOptionsBuilder> optionsAction) => this.optionsAction = optionsAction;
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new ConfigurationDataSourceProvider(optionsAction);
    }
    public class ConfigurationDataSourceProvider : ConfigurationProvider
    {
        public ConfigurationDataSourceProvider(Action<DbContextOptionsBuilder> optionsAction) => OptionsAction = optionsAction;

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
