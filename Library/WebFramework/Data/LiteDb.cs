using LiteDB;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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

            var c = NewConnectionString(connectionString);
            if (init) LiteDatabase = new LiteDatabase(c);
        }

        /// <summary></summary>
        public LiteDb(bool init = true)
        {
            if (init) LiteDatabase = OpenMemory();
        }

        /// <summary></summary>
        public LiteDb(string connectionString, bool init = true)
        {
            var c = NewConnectionString(connectionString);
            if (init) LiteDatabase = new LiteDatabase(c);
        }

        /// <summary></summary>
        public LiteDb(string connectionString, BsonMapper mapper) => LiteDatabase = new LiteDatabase(NewConnectionString(connectionString), mapper);

        /// <summary></summary>
        public LiteDb(ConnectionString connectionString, BsonMapper mapper) => LiteDatabase = new LiteDatabase(GetConnectionString(connectionString), mapper);

        /// <summary></summary>
        public LiteDb(LiteDatabase liteDatabase) => LiteDatabase = liteDatabase;

        /// <summary></summary>
        private ConnectionString NewConnectionString(string connectionString) => GetConnectionString(new ConnectionString(connectionString));
        /// <summary></summary>
        private ConnectionString GetConnectionString(ConnectionString c)
        {
            var s = c.Filename;
            var i = s.LastIndexOf(".");
            var filename = $"{s.Substring(0, i)}{{0}}{s.Substring(i)}";
            _connectionString = c;
            _connectionStringTpl = new ConnectionString()
            {
                Connection = c.Connection,
                Filename = filename,
                Password = c.Password,
                InitialSize = c.InitialSize,
                ReadOnly = c.ReadOnly,
                Upgrade = c.Upgrade,
                Collation = c.Collation,
            };
            return c;
            /// <summary></summary>
        }


        /// <summary>Open database (or create if doesn't exist)</summary>
        public LiteDatabase Open() => new LiteDatabase(_connectionString);
        /// <summary>Open sub database (or create if doesn't exist)</summary>
        public LiteDatabase Open(string s) => new LiteDatabase(GetConnectionString(s));
        /// <summary>Open Memory database (or create if doesn't exist)</summary>
        public LiteDatabase OpenMemory() => new LiteDatabase(new MemoryStream());

        /// <summary></summary>
        public ConnectionString GetConnectionString() => _connectionString;
        /// <summary></summary>
        public ConnectionString GetConnectionString(string s)
        {
            var c = _connectionStringTpl;
            return new ConnectionString()
            {
                Connection = c.Connection,
                Filename = string.Format(c.Filename, s),
                Password = c.Password,
                InitialSize = c.InitialSize,
                ReadOnly = c.ReadOnly,
                Upgrade = c.Upgrade,
                Collation = c.Collation,
            };
        }
        /// <summary></summary>
        public bool HasConnectionString => _connectionString != null && !string.IsNullOrEmpty(_connectionString.Filename);

        /// <summary></summary>
        private ConnectionString _connectionString;
        /// <summary></summary>
        private ConnectionString _connectionStringTpl;

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
