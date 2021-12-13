using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using NATS.Services.Util;
using Newtonsoft.Json;
using RestSharp;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NATS.Services.V8Script
{
    public sealed class DbJs
    {
        public readonly SqlSugarClient Db;

        readonly V8ScriptEngine Engine;

        readonly string Prefix;
        readonly string Subject;

        public string prefix => Prefix;
        public string subject => Subject;

        public DbJs(string connectionString, V8ScriptEngine engine, string prefix, string subject)
        {
            Db = connectionString.NewSqlSugarClient();
            Db.Ado.CommandTimeOut = 120;
            Engine = engine;
            Prefix = prefix;
            Subject = subject;
        }

        public DbJs(Config.DbConfig config, V8ScriptEngine engine, string prefix, string subject)
            : this($"{config.Type}:{config.Conn}", engine, prefix, subject) { }

        /// <summary>
        /// var uuid = $db.uuid()
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object uuid(params object[] args)
        {
            if (args.Length > 0)
                return Guid.NewGuid().ToString(args[0].ToString());
            return Guid.NewGuid().ToString().ToLower();
        }

        /// <summary>
        /// var guid = $db.guid("N")
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object guid(params object[] args)
        {
            if (args.Length > 0)
                return Guid.NewGuid().ToString(args[0].ToString());
            return Guid.NewGuid().ToString().ToLower();
        }

        /// <summary>
        /// var seconds = $db.timeout()
        /// $db.timeout(60)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object timeout(params object[] args)
        {
            if (args.Length > 0)
            {
                if (int.TryParse(args[0].ToString(), out int i) && i > 0)
                    Db.Ado.CommandTimeOut = i;
            }
            return Db.Ado.CommandTimeOut;
        }

        /// <summary>
        /// $db.q: return ResultObject or Array of all rows
        /// $db.q('select * from table1 where id=@id',{id:1})
        /// </summary>
        /// <param name="args"></param>
        public object q(params object[] args)
        {
            if (Db == null || args.Length == 0)
                return null;

            string sql = args[0].ToString();
            if (string.IsNullOrWhiteSpace(sql))
                return null;

            Dictionary<string, object> p = args.Length > 1 ? (args[1] as ScriptObject)?.ToDictionary() : null;

            string code = null;

            var cacheType = CacheObject.Types.Default;
            var cacheEnabled = args.Length > 2 && Enum.TryParse(args[2].ToString(), out cacheType);
            var cacheKey = cacheEnabled ? (sql + p.ToStr()).Md5() : null;
            if (cacheEnabled) code = CacheObject.Get(cacheType, cacheKey);

            if (string.IsNullOrEmpty(code))
            {
                var dt = Db.Ado.GetDataTable(sql, p);
                if (dt == null || dt.Rows.Count == 0)
                    return null;

                code = JsonConvert.SerializeObject(dt, Extensions.JsonConverters);
                if (cacheEnabled) CacheObject.Set(cacheType, cacheKey, code);
            }

            var obj = Engine.Evaluate(JS.SecurityCode(code));
            return obj;
        }

        /// <summary>
        /// $db.q2: return Cache{ Memory = 0, Redis, Default } ResultObject or Array of all rows
        /// $db.q2(0,'select * from table1 where id=@id',{id:1})
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object q2(params object[] args)
        {
            if (Db == null || args.Length <= 1)
                return null;

            string sql = args[1].ToString();
            if (string.IsNullOrWhiteSpace(sql))
                return null;

            Dictionary<string, object> p = args.Length > 2 ? (args[2] as ScriptObject)?.ToDictionary() : null;

            Enum.TryParse(args[0].ToString(), out CacheObject.Types cacheType);
            var cacheKey = (sql + p.ToStr()).Md5();
            var code = CacheObject.Get(cacheType, cacheKey);

            if (string.IsNullOrEmpty(code))
            {
                var dt = Db.Ado.GetDataTable(sql, p);
                if (dt == null || dt.Rows.Count == 0)
                    return null;

                code = JsonConvert.SerializeObject(dt, Extensions.JsonConverters);
                CacheObject.Set(cacheType, cacheKey, code);
            }

            var obj = Engine.Evaluate(JS.SecurityCode(code));
            return obj;
        }

        /// <summary>
        /// $db.g: return ResultValue of first column in first row
        /// $db.g('select name from table1 where id=@id',{id:1})
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object g(params object[] args)
        {
            if (Db == null || args.Length == 0)
                return null;

            string sql = args[0].ToString();
            if (string.IsNullOrWhiteSpace(sql))
                return null;

            Dictionary<string, object> p = args.Length > 1 ? (args[1] as ScriptObject)?.ToDictionary() : null;

            object obj = null;

            var cacheType = CacheObject.Types.Default;
            var cacheEnabled = args.Length > 2 && Enum.TryParse(args[2].ToString(), out cacheType);
            var cacheKey = cacheEnabled ? (sql + p.ToStr()).Md5() : null;
            if (cacheEnabled)
            {
                var code = CacheObject.Get(cacheType, cacheKey);
                if (!string.IsNullOrEmpty(code)) obj = JsonConvert.DeserializeObject(code);
            }

            if (obj == null)
            {
                obj = Db.Ado.GetScalar(sql, p);

                if (obj == null || obj == DBNull.Value)
                    return null;

                if (cacheEnabled) CacheObject.Set(cacheType, cacheKey, JsonConvert.SerializeObject(obj, Extensions.JsonConverters));
            }
            return obj;
        }

        /// <summary>
        /// $db.g2: return Cache{ Memory = 0, Redis, Default } ResultValue of first column in first row
        /// $db.g2(0,'select name from table1 where id=@id',{id:1})
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object g2(params object[] args)
        {
            if (Db == null || args.Length <= 1)
                return null;

            string sql = args[1].ToString();
            if (string.IsNullOrWhiteSpace(sql))
                return null;

            Dictionary<string, object> p = args.Length > 2 ? (args[2] as ScriptObject)?.ToDictionary() : null;

            object obj = null;

            Enum.TryParse(args[0].ToString(), out CacheObject.Types cacheType);
            var cacheKey = (sql + p.ToStr()).Md5();
            var code = CacheObject.Get(cacheType, cacheKey);
            if (!string.IsNullOrEmpty(code)) obj = JsonConvert.DeserializeObject(code);

            if (obj == null)
            {
                obj = Db.Ado.GetScalar(sql, p);

                if (obj == null || obj == DBNull.Value)
                    return null;

                CacheObject.Set(cacheType, cacheKey, JsonConvert.SerializeObject(obj, Extensions.JsonConverters));
            }
            return obj;
        }

        /// <summary>
        /// $db.i: return LastInsertId must int in number-id-column
        /// $db.i('insert into table1 values(@id,@name)',{id:1,name:'test'})
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object i(params object[] args)
        {
            if (Db == null || args.Length == 0)
                return 0;

            if (args.Length > 1 && args[0].GetType().Name.EndsWith("Array"))
            {
                if (Db.CurrentConnectionConfig.DbType != DbType.SqlServer)
                    return 0;

                var dt = new System.Data.DataTable(args[1].ToString());
                var idc = args.Length > 2 ? args[2].ToString() : "Id";

                var list = (dynamic)args[0];
                var leng = list.length;
                var count = 0;
                for (var i = 0; i < leng; i++)
                {
                    var obj = list[i] as ScriptObject;
                    if (obj == null || obj.PropertyNames.Count() == 0) continue;
                    if (dt.Columns.Count == 0)
                    {
                        dt.Columns.Add(idc);
                        foreach (var key in obj.PropertyNames) if (key != idc) dt.Columns.Add(key);
                    }
                    if (dt.Columns.Count > 0)
                    {
                        var row = dt.NewRow();
                        foreach (var key in obj.PropertyNames) if (key != idc) row[key] = obj.GetProperty(key);
                        dt.Rows.Add(row);

                        if (i % 201 == 200 || i + 1 == leng)
                        {
                            count += SQLServer.BatchInsert(dt, Db.Ado.CommandTimeOut, Db.CurrentConnectionConfig.ConnectionString);
                            dt.Clear();
                        }
                    }
                }
                return count;
            }
            else
            {
                string sql = args[0].ToString();
                if (string.IsNullOrWhiteSpace(sql))
                    return 0;

                Dictionary<string, object> p = args.Length > 1 ? (args[1] as ScriptObject)?.ToDictionary() : null;

                var sql0 = sql.ToUpper();
                sql = sql.Trim().TrimEnd(';') + (Db.CurrentConnectionConfig.DbType == DbType.MySql
                ? (sql0.Contains("LAST_INSERT_ID()") ? "" : ";SELECT LAST_INSERT_ID();") : Db.CurrentConnectionConfig.DbType == DbType.SqlServer
                ? (sql0.Contains("SCOPE_IDENTITY()") ? "" : ";SELECT SCOPE_IDENTITY();") : "");
                return Db.Ado.GetScalar(sql, p);
            }
        }

        /// <summary>
        /// $db.x: return RowsAffected all inserted,updated,deleted
        /// $db.x('update table1 set name=@name where id=@id',{id:1,name:'test'})
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object x(params object[] args)
        {
            if (Db == null || args.Length == 0)
                return null;

            string sql = args[0].ToString();
            if (string.IsNullOrWhiteSpace(sql))
                return null;

            Dictionary<string, object> p = args.Length > 1 ? (args[1] as ScriptObject)?.ToDictionary() : null;
            var obj = Db.Ado.ExecuteCommand(sql, p);
            return obj;
        }
    }
}
