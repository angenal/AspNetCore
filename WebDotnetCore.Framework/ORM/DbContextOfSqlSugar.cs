using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace WebFramework.ORM
{
    /// <summary>
    /// 数据表访问基类 http://www.codeisbug.com/Doc/8/
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbSetOfSqlSugar<T> : SimpleClient<T> where T : class, new()
    {
        public DbSetOfSqlSugar(SqlSugarClient context) : base(context)
        {
        }

        #region 扩展方法
        public List<T> GetByIds(dynamic[] ids)
        {
            return Context.Queryable<T>().In(ids).ToList();
        }
        #endregion
    }
    /// <summary>
    /// 数据库访问上下文基类
    /// </summary>
    public class DbContextOfSqlSugar
    {
        public SqlSugarClient Db;
        public DbContextOfSqlSugar(IOptions<ConnectionConfig> options)
        {
            Db = new SqlSugarClient(options.Value);
        }
        //public DbSetOfSqlSugar<Student> Students { get { return new DbSetOfSqlSugar<Student>(Db); } }
    }
    /// <summary>
    /// 把数据库访问类 注册到服务容器中
    /// </summary>
    public static class SqlSugarServiceCollectionExtensions
    {
        public static IServiceCollection AddDbContextOfSqlSugar<TContext>(this IServiceCollection serviceCollection,
            Action<ConnectionConfig> optionsAction = null) where TContext : DbContextOfSqlSugar
        {
            //WebCore.Utils.Check.NotNull(serviceCollection, "serviceCollection");

            if (optionsAction != null)
                serviceCollection.Configure(optionsAction);

            serviceCollection.AddScoped<TContext>();

            return serviceCollection;
        }
    }
    public static class SqlSugarExtensions
    {
        //public static IApplicationBuilder UseSqlSugar(this IApplicationBuilder builder)
        //{
        //    return builder.UseMiddleware<SqlSugarMiddleware>();
        //}
    }
}
