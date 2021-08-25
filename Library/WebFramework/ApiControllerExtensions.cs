using SqlSugar;
using System;
using System.Text;

namespace WebFramework
{
    /// <summary>
    /// 数据库 扩展方法
    /// </summary>
    public static class SqlSugarExtensions
    {
        /// <summary>
        /// new SqlSugarClient
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static SqlSugarClient NewSqlSugarClient(string connectionString)
        {
            var db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = connectionString,
                DbType = DbType.SqlServer,
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true,
            });
#if DEBUG
            return db.Debug();
#else
            db.Ado.IsEnableLogEvent = false;
            return db;
#endif
        }

        /// <summary>
        /// 调式代码,打印SQL
        /// </summary>
        /// <param name="db"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static SqlSugarClient Debug(this SqlSugarClient db, Action<string> action = null)
        {
            if (db == null) return db;
            if (action == null) action = sql => System.Diagnostics.Debug.WriteLine(sql);
            if (db.Aop != null) db.Aop.OnLogExecuting = (sql, pars) => action.Invoke("  " + sql + Environment.NewLine + WriteLineSugarParameter(pars));
            return db;
        }
        static string WriteLineSugarParameter(SugarParameter[] pars)
        {
            if (pars == null) return null;
            var s = new StringBuilder();
            foreach (SugarParameter p in pars) s.Append($"    {p.ParameterName} = {p.Value} {Environment.NewLine}");
            return s.ToString();
        }

        //public static IApplicationBuilder UseSqlSugar(this IApplicationBuilder builder) => builder.UseMiddleware<SqlSugarMiddleware>();
    }
}
