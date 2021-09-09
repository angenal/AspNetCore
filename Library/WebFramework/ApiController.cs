using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.Collections.Generic;
using WebFramework.Data;

namespace WebFramework
{
    /// <summary>
    /// Api ControllerBase Extensions
    /// </summary>
    public class ApiController : ControllerBase
    {
        /// <summary>
        /// Current user info
        /// </summary>
        public Session user;

        /// <summary>
        /// new SqlSugarClient
        /// </summary>
        public SqlSugarClient db => _db ??= SQLServerDb.DefaultConnection.NewSqlSugarClient(":", SqlSugarClientDebug);
        private SqlSugarClient _db;
        private void SqlSugarClientDebug(string sql)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(sql);
#endif
            var trace = HttpContext.TraceIdentifier;
            if (trace != null) HttpContext.Items.TryAdd(trace + "sql", sql);
        }

        /// <summary></summary>
        public ApiController() { }

        /// <summary></summary>
        protected BadRequestObjectResult Error(string title, int status = 400)
        {
            return BadRequest(new ErrorJsonResultObject { Status = status, Title = title });
        }
        /// <summary></summary>
        protected BadRequestObjectResult Error(string title, string detail, int status = 400)
        {
            return BadRequest(new ErrorJsonResultObject { Status = status, Title = title, Detail = detail });
        }
    }
}
