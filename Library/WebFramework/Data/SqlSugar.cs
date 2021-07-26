using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlSugar;
using System;

namespace WebFramework.Data
{
    /// <summary>
    /// 数据表 基类 https://www.donet5.com/home/doc
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbTable<T> : SimpleClient<T> where T : class, new()
    {
        public DbTable(ISqlSugarClient context = null, ConnectionStrings connection = null) : base(context)
        {
            if (context != null || connection == null) return;
            Context = new SqlSugarClient(new ConnectionConfig()
            {
                DbType = DbType.SqlServer,
                ConnectionString = connection.DefaultConnection,
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true,
            });
        }
    }
    /// <summary>
    /// 数据库 访问上下文
    /// </summary>
    public class DbContextOfSqlSugar
    {
        public SqlSugarClient Client;
        public DbContextOfSqlSugar(IOptions<ConnectionConfig> options) => Client = new SqlSugarClient(options.Value);
    }
    /// <summary>
    /// 把数据库访问类 注册到服务容器中
    /// </summary>
    public static class SqlSugarServiceCollectionExtensions
    {
        public static IServiceCollection AddDbContextOfSqlSugar<TContext>(this IServiceCollection services,
            Action<ConnectionConfig> optionsAction = null) where TContext : DbContextOfSqlSugar
        {
            if (optionsAction != null) services.Configure(optionsAction);

            services.AddScoped<TContext>();

            return services;
        }
    }
    //public static class SqlSugarExtensions
    //{
    //    public static IApplicationBuilder UseSqlSugar(this IApplicationBuilder builder)
    //    {
    //        return builder.UseMiddleware<SqlSugarMiddleware>();
    //    }
    //}
}
