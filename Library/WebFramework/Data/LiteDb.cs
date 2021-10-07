using LiteDB;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using WebInterface;

namespace WebFramework.Data
{
    /// <summary>
    /// LiteDB database service.
    /// </summary>
    public class LiteDb : ILiteDb
    {
        /// <summary></summary>
        public LiteDb(IConfiguration config, string connectionStringName = null, bool init = true)
        {
            string connectionString;
            try
            {
                if (connectionStringName != null)
                {
                    connectionString = Environment.GetEnvironmentVariable(connectionStringName);
                    if (string.IsNullOrEmpty(connectionString)) connectionString = config.GetConnectionString(connectionStringName);
                }
                else
                {
                    connectionString = config.GetSection("ConnectionStrings").GetChildren().First().Value;
                }
            }
            catch (Exception)
            {
                throw new NullReferenceException("No connection string defined in appsettings.json");
            }

            _connectionString = connectionString;

            if (init) LiteDatabase = new LiteDatabase(connectionString);
        }

        /// <summary></summary>
        public LiteDb(bool init = true)
        {
            if (init) LiteDatabase = OpenMemory();
        }

        /// <summary></summary>
        public LiteDb(string connectionString, bool init = true)
        {
            _connectionString = connectionString;
            if (init) LiteDatabase = new LiteDatabase(connectionString);
        }

        /// <summary></summary>
        public LiteDb(string connectionString, BsonMapper mapper) => LiteDatabase = new LiteDatabase(connectionString, mapper);

        /// <summary></summary>
        public LiteDb(ConnectionString connectionString, BsonMapper mapper) => LiteDatabase = new LiteDatabase(connectionString, mapper);

        /// <summary></summary>
        public LiteDb(LiteDatabase liteDatabase) => LiteDatabase = liteDatabase;


        /// <summary>Open Current ConnectionString Database</summary>
        public LiteDatabase Open() => new LiteDatabase(_connectionString);
        /// <summary>Open Memory Database</summary>
        public LiteDatabase OpenMemory() => new LiteDatabase(new MemoryStream());

        /// <summary></summary>
        public string GetConnectionString() => _connectionString;
        /// <summary></summary>
        public bool HasConnectionString => !string.IsNullOrEmpty(_connectionString);

        private readonly string _connectionString;

        /// <summary></summary>
        public LiteDatabase LiteDatabase { get; protected set; }
    }

    /// <summary>
    /// LiteDB extensions.
    /// </summary>
    public static class LiteDbExtensions
    {
        /// <summary></summary>
        public static ILiteQueryable<T> WhereIF<T>(this ILiteQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition ? query.Where(predicate) : query;
        }
    }
}
