using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebFramework.Configurations
{
    /// <summary>
    /// 数据库中的任何设置都将替代 appsettings.json 文件中的设置
    /// </summary>
    public static class EFConfigurationExtensions
    {
        /// <summary>
        /// new ConfigurationBuilder().AddEFConfigurationSource(
        ///     options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")))
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="optionsAction"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddEFConfigurationSource(this IConfigurationBuilder builder, Action<DbContextOptionsBuilder> optionsAction)
        {
            return builder.Add(new EFConfigurationSource(optionsAction));
        }
    }
    public class EFConfigurationSource : IConfigurationSource
    {
        private readonly Action<DbContextOptionsBuilder> optionsAction;
        public EFConfigurationSource(Action<DbContextOptionsBuilder> optionsAction)
        {
            this.optionsAction = optionsAction;
        }
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EFConfigurationProvider(optionsAction);
        }
    }
    public class EFConfigurationProvider : ConfigurationProvider
    {
        public EFConfigurationProvider(Action<DbContextOptionsBuilder> optionsAction)
        {
            OptionsAction = optionsAction;
        }

        Action<DbContextOptionsBuilder> OptionsAction { get; }

        /// <summary>
        /// Load config data from DB
        /// </summary>
        public override void Load()
        {
            var builder = new DbContextOptionsBuilder<ConfigurationContext>();
            OptionsAction(builder);
            using (var dbContext = new ConfigurationContext(builder.Options))
            {
                dbContext.Database.EnsureCreated(); // 如果没有建立db,会自动创建
                Data = !dbContext.Values.Any()
                    ? CreateAndSaveDefaultValues(dbContext)
                    : dbContext.Values.ToDictionary(c => c.Id, c => c.Value);
            }
        }
        private static IDictionary<string, string> CreateAndSaveDefaultValues(ConfigurationContext dbContext)
        {
            var initialData = new Dictionary<string, string>();
            if (initialData.Count > 0)
            {
                dbContext.Values.AddRange(initialData
                    .Select(kv => new ConfigurationValue { Id = kv.Key, Value = kv.Value })
                    .ToArray());
                dbContext.SaveChanges();
            }
            return initialData;
        }
    }
    public class ConfigurationContext : DbContext
    {
        public ConfigurationContext(DbContextOptions options) : base(options) { }
        public DbSet<ConfigurationValue> Values { get; set; }
    }
    public class ConfigurationValue
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }
}
