using LiteDB;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Identity.LiteDB.Data
{
    public class LiteDbContext : ILiteDbContext
    {
        public LiteDbContext(IConfiguration config, string connectionStringName = null)
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

            LiteDatabase = new LiteDatabase(connectionString);
        }

        public LiteDbContext() => LiteDatabase = OpenMemory();

        public LiteDbContext(string connectionString, BsonMapper mapper = null) => LiteDatabase = new LiteDatabase(connectionString, mapper);

        public LiteDbContext(ConnectionString connectionString, BsonMapper mapper = null) => LiteDatabase = new LiteDatabase(connectionString, mapper);

        public LiteDbContext(LiteDatabase liteDatabase) => LiteDatabase = liteDatabase;


        public LiteDatabase Open() => new LiteDatabase(_connectionString);
        public LiteDatabase OpenMemory() => new LiteDatabase(new System.IO.MemoryStream());

        public string GetConnectionString() => _connectionString;

        private readonly string _connectionString;
        public LiteDatabase LiteDatabase { get; protected set; }
    }
}
