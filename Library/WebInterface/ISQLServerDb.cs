using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebInterface
{
    /// <summary>
    /// Microsoft SQL Server database interface.
    /// </summary>
    public interface ISQLServerDb
    {
        /// <summary>
        /// Value for SQL statements that return a single row with one or more columns.
        /// </summary>
        Task<T> Value<T>(string query, object parameters = null);

        /// <summary>
        /// List for SQL statements that return a single row with one or more columns.
        /// </summary>
        Task<List<T>> List<T>(string query, object parameters = null);

        /// <summary>
        /// Json to get the result of an SQL statement as JSON (JObject).
        /// </summary>
        Task<JObject> Json(string query, object parameters = null);

        /// <summary>
        /// JsonArray to get the result of an SQL statement as a JSON Array (JArray).
        /// </summary>
        Task<JArray> JsonArray(string query, object parameters = null);

        /// <summary>
        /// Execute for SQL statements that don't return results: INSERT, UPDATE, DELETE, etc.
        /// </summary>
        Task<int> Execute(string query, object parameters = null);
    }
}
