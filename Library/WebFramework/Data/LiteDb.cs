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

        public LiteDb(bool init = true)
        {
            if (init) LiteDatabase = OpenMemory();
        }

        public LiteDb(string connectionString, bool init = true)
        {
            _connectionString = connectionString;
            if (init) LiteDatabase = new LiteDatabase(connectionString);
        }

        public LiteDb(string connectionString, BsonMapper mapper) => LiteDatabase = new LiteDatabase(connectionString, mapper);

        public LiteDb(ConnectionString connectionString, BsonMapper mapper) => LiteDatabase = new LiteDatabase(connectionString, mapper);

        public LiteDb(LiteDatabase liteDatabase) => LiteDatabase = liteDatabase;


        public LiteDatabase Open() => new LiteDatabase(_connectionString);
        public LiteDatabase OpenMemory() => new LiteDatabase(new MemoryStream());

        public string GetConnectionString() => _connectionString;
        public bool HasConnectionString => !string.IsNullOrEmpty(_connectionString);

        private readonly string _connectionString;

        public LiteDatabase LiteDatabase { get; protected set; }
    }
}
