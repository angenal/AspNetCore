using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebFramework.Orm;
using WebInterface;

namespace WebFramework.Services
{
    /// <summary>
    /// Database Module
    /// </summary>
    public static class DatabaseModule
    {
        /// <summary>
        /// Register services
        /// </summary>
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
        {
            // Database: InMemory
            services.AddDbContext<ValuesDbContext>(options => options.UseInMemoryDatabase("Values"));
            // Database: Sqlite
            var sqliteConnectionString = config.GetConnectionString("Sqlite");
            if (!string.IsNullOrEmpty(sqliteConnectionString))
            {
                services.AddDbContextOfSqlSugar<ValuesContextOfSqlSugar>(options =>
                {
                    options.DbType = SqlSugar.DbType.Sqlite;
                    options.ConnectionString = sqliteConnectionString;
                    options.IsAutoCloseConnection = true;
                });
            }
            // Database: LiteDB (similar to sqlite)
            var liteDb = new LiteDb(config, "LiteDB");
            if (liteDb.HasConnectionString) services.AddSingleton<ILiteDb, LiteDb>(_ => liteDb);
            // Database: Microsoft SQL Server Client
            var mssqlDb = new SQLServerDb(config, "DefaultConnection");
            if (mssqlDb.HasConnectionString) services.AddSingleton<ISQLServerDb, SQLServerDb>(_ => mssqlDb);
            //// Database: PostgreSQL Client - Depend on Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.UnitOfWork
            //var pgsqlDb = new PostgreSqlDb(config, "PostgreDB");
            //var connectionString = pgsqlDb.GetConnectionString();
            //if (connectionString != null) services.AddDbContext<Models.TestDbContext>(options => options.UseNpgsql(connectionString)).AddUnitOfWork<Models.TestDbContext>();
            //// Database: throw error pages to detect and diagnose errors with Entity Framework Core migrations.
            //services.AddDatabaseDeveloperPageExceptionFilter();

            return services;
        }
    }
}
