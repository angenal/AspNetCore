using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebInterface;

namespace WebFramework
{
    public sealed class SQLServerDb : ISQLServerDb
    {
        private readonly string _connectionString;

        public SQLServerDb(IConfiguration config, string connectionStringName = "DefaultConnection")
        {
            var connectionString = Environment.GetEnvironmentVariable(connectionStringName);
            if (string.IsNullOrEmpty(connectionString)) connectionString = config.GetConnectionString(connectionStringName);
            _connectionString = connectionString;
        }

        public SQLServerDb(string connectionString) => _connectionString = connectionString;

        public string GetConnectionString() => _connectionString;
        public bool HasConnectionString => !string.IsNullOrEmpty(_connectionString);

        public async Task<T> Value<T>(string query, object parameters = null)
        {
            using var conx = new SqlConnection(_connectionString);
            return await conx.QueryFirstOrDefaultAsync<T>(query, parameters);
        }

        public async Task<List<T>> List<T>(string query, object parameters = null)
        {
            using var conx = new SqlConnection(_connectionString);
            var results = await conx.QueryAsync<T>(query, parameters);
            return results.ToList();
        }

        public async Task<JObject> Json(string query, object parameters = null)
        {
            var result = await Value<dynamic>(query, parameters);
            return JObject.FromObject(result);
        }

        public async Task<JArray> JsonArray(string query, object parameters = null)
        {
            var result = await List<dynamic>(query, parameters);
            return JArray.FromObject(result);
        }

        public async Task<int> Execute(string query, object parameters = null)
        {
            using var conx = new SqlConnection(_connectionString);
            return await conx.ExecuteAsync(query, parameters);
        }
    }
}
