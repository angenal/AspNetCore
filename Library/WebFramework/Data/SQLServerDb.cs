using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebInterface;

namespace WebFramework.Data
{
    /// <summary></summary>
    public sealed class SQLServerDb : ISQLServerDb
    {
        private readonly string _connectionString;

        /// <summary></summary>
        public static string DefaultConnection;

        /// <summary></summary>
        public SQLServerDb(IConfiguration config, string connectionStringName = "DefaultConnection")
        {
            var connectionString = Environment.GetEnvironmentVariable(connectionStringName);
            if (string.IsNullOrEmpty(connectionString)) connectionString = config.GetConnectionString(connectionStringName);
            if (DefaultConnection == null) DefaultConnection = connectionString;
            _connectionString = connectionString;
        }

        /// <summary></summary>
        public SQLServerDb(string connectionString) => _connectionString = connectionString;

        /// <summary></summary>
        public string GetConnectionString() => _connectionString;
        /// <summary></summary>
        public bool HasConnectionString => !string.IsNullOrEmpty(_connectionString);

        /// <summary></summary>
        public SqlConnection Cnn { get => new SqlConnection(_connectionString); }

        /// <summary></summary>
        public async Task<T> Value<T>(string query, object parameters = null)
        {
            using var cnn = Cnn;
            return await cnn.QueryFirstOrDefaultAsync<T>(query, parameters);
        }

        /// <summary></summary>
        public async Task<List<T>> List<T>(string query, object parameters = null)
        {
            using var cnn = Cnn;
            var results = await cnn.QueryAsync<T>(query, parameters);
            return results.ToList();
        }

        /// <summary></summary>
        public async Task<JObject> Json(string query, object parameters = null)
        {
            var result = await Value<dynamic>(query, parameters);
            return JObject.FromObject(result);
        }

        /// <summary></summary>
        public async Task<JArray> JsonArray(string query, object parameters = null)
        {
            var result = await List<dynamic>(query, parameters);
            return JArray.FromObject(result);
        }

        /// <summary></summary>
        public async Task<int> Execute(string query, object parameters = null)
        {
            using var cnn = Cnn;
            return await cnn.ExecuteAsync(query, parameters);
        }
    }
}
