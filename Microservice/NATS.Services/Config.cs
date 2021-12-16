using Hangfire;
using Hangfire.Redis;
using Microsoft.ClearScript;
using NATS.Client;
using Newtonsoft.Json;
using Serilog;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WebCore;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NATS.Services
{
    /// <summary>
    /// 系统配置 natsql.yaml
    /// </summary>
    public class Config : ICloneable
    {
        #region 基本结构字段定义

        /// <summary>
        /// Identity.
        /// </summary>
        protected uint id { get; set; }
        /// <summary>
        /// Id column.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 数据库client
        /// </summary>
        public DbConfig Db { get; set; }
        /// <summary>
        /// 消息中间件/发布订阅处理
        /// </summary>
        public NatsConfig Nats { get; set; }
        private IConnection natsConnection { get; set; }
        private IAsyncSubscription natSubscription { get; set; }
        /// <summary>
        /// 连接redis
        /// </summary>
        public RedisConfig Redis { get; set; }

        public string Dir { get; set; }
        public string Js { get; set; }
        public string JsContent { get; set; }
        public string Subject { get; set; }

        public V8Script.JS JS { get; internal set; }
        public string JsFunction { get; set; } = "sql";
        public Handler JsHandler { get; internal set; }

        public const string JsDefaultFile = "natsql.js";

        #endregion

        /// <summary>
        /// 全部配置项目
        /// </summary>
        public static readonly List<Config> Items = new List<Config>();

        /// <summary>
        /// 最新项目时间戳
        /// </summary>
        /// <returns></returns>
        public static long LatestCreated() => Items.OrderByDescending(c => c.Nats.Created).First().Nats.Created;

        ///// <summary>
        ///// 后台定时任务调试器
        ///// </summary>
        //public static IScheduler JsCronScheduler;
        public bool IsJsCron { get; internal set; }
        //public Tuple<IJobDetail, ITrigger> JsCronJob { get; internal set; }

        #region 任务调试模板定义

        bool IsJsTemplate() => Nats.Script.EndsWith(".js");
        bool IsDbTemplate() => Nats.Script.EndsWith("db");

        string TemplatePath()
        {
            var jsPath = Nats.Script;
            if (!File.Exists(jsPath)) jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Nats.Script);
            if (!File.Exists(jsPath)) jsPath = Path.Combine(Environment.CurrentDirectory, Nats.Script);
            return jsPath;
        }

        #endregion

        /// <summary>
        /// 动态加载配置项目
        /// </summary>
        Thread JsDynamicLoad { get; set; }

        /// <summary>
        /// 解析 natsql.yaml
        /// </summary>
        public static Config Parse(string configFile)
        {
            var s = File.ReadAllText(configFile);
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var c = deserializer.Deserialize<Config>(s);

            //JsCronScheduler = QuartzHelper.Create(c.Nats.Subscribe).GetScheduler().GetAwaiter().GetResult();
            //JsCronScheduler.Start();

            return c;
        }

        /// <summary>
        /// 初始化配置
        /// </summary>
        public int Init(long created, int loadInterval, bool reload = false)
        {
            // create latest config items
            var items = new List<Config>();

            // get created config items
            var createdSubjects = Items.Select(c => c.Subject).ToList();

            // enable redis
            Util.Redis.Init(Redis);

            // enable hangfire 后台定时任务调试器
            Util.Redis.Configure(configuration =>
            {
                // HangFire.Redis.StackExchange  https://github.com/marcoCasamento/Hangfire.Redis.StackExchange
                GlobalConfiguration.Configuration.UseRedisStorage(configuration, new RedisStorageOptions
                {
                    Db = Redis.Db,
                    //DeletedListSize = 1000,
                    //SucceededListSize = 10000,
                    //InvisibilityTimeout = TimeSpan.FromMinutes(5),       // 超时后由另一个工作进程接手该后台作业任务（重新加入）默认5分钟
                    Prefix = $"hangfire:{Nats.Subscribe}",
                });
            });

            // init js template
            if (IsJsTemplate())
            {
                var jsPath = TemplatePath();
                if (!File.Exists(jsPath)) return items.Count;

                var jsContent = File.ReadAllText(jsPath);
                var id = jsContent.Crc32();
                if (reload && id == this.id) return items.Count;
                this.id = id;

                var jst = new V8Script.JS(jsContent, Db, Redis, null, null, null, true, true);

                var subItems = jst.Engine.Script.subscribe;
                if (subItems == null) throw new Exception("配置订阅任务\"subscribe\" 异常！");

                string dir1 = string.IsNullOrEmpty(Dir) ? Nats.Subscribe : Dir, js1 = string.IsNullOrEmpty(Js) ? JsDefaultFile : Js;

                foreach (var item in subItems)
                {
                    var conf = (Config)Clone();
                    // subscription item
                    string name = item.name, spec = item.spec, dir = item.func();
                    // subscription name
                    string subject = "+" == spec ? Nats.Subscribe + name : name, itemDir = $"{dir1}/{dir}";

                    // it's Job And Cron Expressions
                    conf.IsJsCron = spec.Split(' ').Length >= 5;
                    if (conf.IsJsCron) subject = Nats.Subscribe + name;

                    conf.Nats.Name = name; conf.Nats.Subscribe = subject; conf.Nats.Spec = spec;
                    if (createdSubjects.Contains(subject)) continue;
                    conf.Subject = subject;
                    conf.Dir = itemDir;
                    // script file content
                    string itemJs = $"{itemDir}/{js1}";

                    conf.Nats.Created = File.GetCreationTime(itemJs).ToFileTime();
                    if (created >= conf.Nats.Created) continue;
                    conf.Nats.Version = File.GetLastWriteTime(itemJs).ToFileTime();
                    conf.Nats.CacheDir = itemDir;
                    conf.Nats.MsgLimit = 100000000;
                    conf.Nats.BytesLimit = 1024;
                    //conf.Nats.Amount = 0;
                    //conf.Nats.Bulk = 200;
                    //conf.Nats.Interval = 2000;

                    string itemJsContent = File.ReadAllText(itemJs);
                    conf.Js = itemJs;
                    conf.JsContent = itemJsContent;

                    // script handler
                    var jsHandler = conf.NewJsHandler();

                    var js = new V8Script.JS(itemJsContent, Db, Redis, jsHandler.Connection, Nats.Subscribe, subject, true, false);
                    js.Add("config", conf.Clone());
                    js.Engine.Execute(js.Script);

                    if (js.Engine.Script.sql == null) throw new Exception("配置function sql(records) 异常！");

                    conf.JS = js;
                    conf.JsHandler = jsHandler;

                    if (conf.IsJsCron)
                    {
                        //conf.JsCronJob = QuartzHelper.New<Handler>(conf.Subject, spec, null, conf.JsFunction);
                        //JsCronScheduler.ScheduleJob(conf.JsCronJob.Item1, conf.JsCronJob.Item2);
                        RecurringJob.AddOrUpdate(() => conf.JS.Engine.Invoke(conf.JsFunction, DateTime.Now), spec, TimeZoneInfo.Local, conf.Subject);
                    }

                    if (loadInterval > 0)
                    {
                        conf.JsDynamicLoad = new Thread(new ParameterizedThreadStart(conf.DynamicLoad)) { IsBackground = true };
                        conf.JsDynamicLoad.Start(loadInterval);
                    }

                    items.Add(conf);
                }
            }

            // init db template
            if (IsDbTemplate())
            {
                System.Data.DataTable dt;
                if (string.IsNullOrEmpty(Nats.Table)) Nats.Table = "subscribes";
                if (Nats.Script.EndsWith(".db"))
                {
                    // connect sqlite db
                    var dbPath = TemplatePath();
                    if (!File.Exists(dbPath)) return items.Count;
                    var db = new SqlSugarClient(new ConnectionConfig
                    {
                        ConnectionString = $"Data Source={dbPath};Mode=ReadOnly", // Data Source=natsql.db;Password=123456
                        DbType = DbType.Sqlite,
                        InitKeyType = InitKeyType.Attribute,
                        IsAutoCloseConnection = true,
                    });
                    dt = db.Ado.GetDataTable($"Select * From {Nats.Table} Where Deleted=0 and Created>{created}");
                }
                else
                {
                    // connect other db
                    var db = new SqlSugarClient(new ConnectionConfig
                    {
                        ConnectionString = Db.Conn,
                        DbType = Db.Type == "mysql" ? DbType.MySql : DbType.SqlServer,
                        InitKeyType = InitKeyType.Attribute,
                        IsAutoCloseConnection = true,
                    });
                    dt = db.Ado.GetDataTable($"Select * From {Nats.Table} Where Deleted=0 and Created>{created}");
                    db.Dispose();
                }
                if (dt == null || dt.Rows.Count == 0) return items.Count;

                string dir1 = string.IsNullOrEmpty(Dir) ? Nats.Subscribe : Dir, js1 = string.IsNullOrEmpty(Js) ? JsDefaultFile : Js;
                foreach (System.Data.DataRow dr in dt.Rows)
                {
                    var conf = (Config)Clone();
                    conf.Nats.Created = Convert.ToInt64(dr["Created"]);
                    if (created >= conf.Nats.Created) continue;
                    if (dt.Columns.Contains("Id")) conf.Id = Convert.ToString(dr["Id"]);
                    conf.Nats.Version = Convert.ToInt64(dr["Version"]);
                    conf.Nats.CacheDir = $"{dir1}/{dr["CacheDir"]}";
                    conf.Nats.MsgLimit = 100000000;
                    conf.Nats.BytesLimit = 1024;
                    var msgLimit = Convert.ToInt32(dr["MsgLimit"]); if (msgLimit > 0) conf.Nats.MsgLimit = msgLimit;
                    var bytesLimit = Convert.ToInt32(dr["BytesLimit"]); if (bytesLimit > 0) conf.Nats.BytesLimit = bytesLimit;
                    var amount = Convert.ToInt32(dr["Amount"]); if (amount > 0) conf.Nats.Amount = amount;
                    var bulk = Convert.ToInt32(dr["Bulk"]); if (bulk > 0) conf.Nats.Bulk = bulk;
                    var interval = Convert.ToInt32(dr["Interval"]); if (interval > 0) conf.Nats.Interval = interval;

                    // subscription item
                    string name = Convert.ToString(dr["Name"]), spec = Convert.ToString(dr["Spec"]), dir = Convert.ToString(dr["Func"]);
                    // subscription name
                    string subject = "+" == spec ? Nats.Subscribe + name : name, itemDir = $"{dir1}/{dir}";

                    // it's Job And Cron Expressions
                    conf.IsJsCron = spec.Split(' ').Length >= 5;
                    if (conf.IsJsCron) subject = Nats.Subscribe + name;

                    conf.Nats.Name = name; conf.Nats.Subscribe = subject; conf.Nats.Spec = spec;
                    if (createdSubjects.Contains(subject)) continue;
                    conf.Subject = subject;
                    conf.Dir = itemDir;
                    // script file content
                    string itemJs = $"{itemDir}/{js1}", itemJsContent = Convert.ToString(dr["Content"]);
                    conf.Js = itemJs;
                    conf.JsContent = itemJsContent;
                    // script handler
                    var jsHandler = conf.NewJsHandler();

                    var js = new V8Script.JS(itemJsContent, Db, Redis, jsHandler.Connection, Nats.Subscribe, subject, true, false);
                    js.Add("config", conf.Clone());
                    js.Engine.Execute(js.Script);

                    if (js.Engine.Script.sql == null) throw new Exception("配置function sql(records) 异常！");

                    conf.JS = js;
                    conf.JsHandler = jsHandler;

                    if (conf.IsJsCron)
                    {
                        //conf.JsCronJob = QuartzHelper.New<Handler>(conf.Subject, spec, null, conf.JsFunction);
                        //JsCronScheduler.ScheduleJob(conf.JsCronJob.Item1, conf.JsCronJob.Item2);
                        RecurringJob.AddOrUpdate(() => conf.JS.Engine.Invoke(conf.JsFunction, DateTime.Now), spec, TimeZoneInfo.Local, conf.Subject);
                    }

                    if (loadInterval > 0)
                    {
                        conf.JsDynamicLoad = new Thread(new ParameterizedThreadStart(conf.DynamicLoad)) { IsBackground = true };
                        conf.JsDynamicLoad.Start(loadInterval);
                    }

                    items.Add(conf);
                }
            }

            if (0 < items.Count)
            {
                // 添加新的调度任务
                Items.AddRange(items);
                // 初始化动态调度任务
                InitNatsConnection();
            }

            return items.Count;
        }

        #region 动态调度任务

        /// <summary>
        /// 查询调度任务
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        string SelectItems(Subscribes data)
        {
            var items = Items.Where(i => i.Nats.Name == data.Name).Select(i => new Subscribes() { Id = i.Id, Name = i.Nats.Name, Spec = i.Nats.Spec, Func = i.JsFunction, Content = i.JsContent, CacheDir = i.Nats.CacheDir, MsgLimit = i.Nats.MsgLimit, BytesLimit = i.Nats.BytesLimit, Amount = i.Nats.Amount, Bulk = i.Nats.Bulk, Interval = i.Nats.Interval, Version = i.Nats.Version, Created = i.Nats.Created, Deleted = i.Nats.Deleted });
            var result = !items.Any() ? "" : JsonConvert.SerializeObject(items.First());
            return result;
        }

        /// <summary>
        /// 新增调度任务
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        string InsertItems(Subscribes data)
        {
            // insert js template
            if (IsJsTemplate())
            {
                var jsPath = TemplatePath();
                if (!File.Exists(jsPath)) return "";

                string dir1 = string.IsNullOrEmpty(Dir) ? Nats.Subscribe : Dir, js1 = string.IsNullOrEmpty(Js) ? JsDefaultFile : Js;
                if (string.IsNullOrEmpty(data.CacheDir)) data.CacheDir = data.Name;
                string itemDir = $"{dir1}/{data.CacheDir}";
                if (!Directory.Exists(itemDir)) Directory.CreateDirectory(itemDir);
                // script file content
                string itemJs = $"{itemDir}/{js1}";
                File.WriteAllText(itemJs, data.Content, Encoding.UTF8);

                var jsContent = File.ReadAllText(jsPath);
                var s = new StringBuilder();
                foreach (string line in jsContent.Split(Environment.NewLine.ToCharArray()))
                {
                    if (!line.StartsWith("];"))
                    {
                        s.AppendLine(line);
                        continue;
                    }
                    s.AppendLine("    {");
                    s.AppendLine("        name: \"" + data.Name + "\",");
                    s.AppendLine("        spec: \"" + data.Spec + "\",");
                    s.AppendLine("        func: function () { return this.name; }");
                    s.AppendLine("    },");
                    s.AppendLine("];");
                }

                File.WriteAllText(jsPath, s.ToString(), Encoding.UTF8);
                return "ok";
            }

            // insert db template
            if (IsDbTemplate())
            {
                if (JS.Database.Db == null) return "";

                if (data.Created <= 0) data.Created = DateTimeOffset.Now.ToUnixTimeSeconds();
                var sql = $"INSERT INTO {Nats.Table} ({(string.IsNullOrEmpty(data.Id) ? "" : "Id,")}Name,Spec,Func,Content,CacheDir,MsgLimit,BytesLimit,Amount,Bulk,Interval,Created) ";
                sql += $"VALUES ({(string.IsNullOrEmpty(data.Id) ? "" : "@Id,")}@Name,@Spec,@Func,@Content,@CacheDir,@MsgLimit,@BytesLimit,@Amount,@Bulk,@Interval,@Created)";
                var v = JS.Database.Db.Ado.ExecuteCommand(sql, data);
                if (0 < v) return "ok";
            }
            return "";
        }

        /// <summary>
        /// 修改调度任务
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        string UpdateItems(Subscribes data)
        {
            // update js template
            if (IsJsTemplate())
            {
                // script file content
                string itemJs = Js;
                if (File.Exists(itemJs))
                {
                    File.WriteAllText(itemJs, data.Content, Encoding.UTF8);
                    return "ok";
                }
            }
            // update db template
            if (IsDbTemplate())
            {
                if (JS.Database.Db == null) return "";

                var v = JS.Database.Db.Ado.ExecuteCommand($"Update {Nats.Table} Set Content=@Content Where Name=@Name", new { data.Name, data.Content });
                if (0 < v) return "ok";
            }
            return "";
        }

        /// <summary>
        /// 删除调度任务
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        string DeleteItems(Subscribes data)
        {
            // update js template
            if (IsJsTemplate())
            {
                // script file content
                string itemJs = Js, backup = ".bak";
                if (data.Deleted && File.Exists(itemJs) && !File.Exists(itemJs + backup))
                {
                    File.Copy(itemJs, itemJs + backup);
                    File.Delete(itemJs);
                    return "ok";
                }
                if (!data.Deleted && !File.Exists(itemJs) && File.Exists(itemJs + backup))
                {
                    File.Copy(itemJs + backup, itemJs);
                    File.Delete(itemJs + backup);
                    return "ok";
                }
            }
            // update db template
            if (IsDbTemplate())
            {
                if (JS.Database.Db == null) return "";

                var v = JS.Database.Db.Ado.ExecuteCommand($"Update {Nats.Table} Set Deleted=@Deleted Where Name=@Name", new { data.Name, data.Deleted });
                if (0 < v) return "ok";
            }
            return "";
        }

        /// <summary>
        /// 初始化动态调度任务
        /// </summary>
        void InitNatsConnection()
        {
            if (natsConnection != null || Items.Count == 0) return;

            natsConnection = Items[0].JsHandler.NewConnection();

            var natsSubject = Nats.Subscribe;
            natSubscription = natsConnection.SubscribeAsync(natsSubject, (sender, e) =>
            {
                if (e.Message?.Data == null) return;
                var result = HandleNatsSubscribe(Encoding.UTF8.GetString(e.Message.Data));
                e.Message.Respond(Encoding.UTF8.GetBytes(result));
            });
            natSubscription.SetPendingLimits(1000000, 1000000 * 1024);
        }

        string HandleNatsSubscribe(string data)
        {
            string result = "";
            try
            {
                switch (data)
                {
                    case "select":
                        var items = Items.Select(i => new Subscribes() { Id = i.Id, Name = i.Nats.Name, Spec = i.Nats.Spec, Func = i.JsFunction, Content = i.JsContent, CacheDir = i.Nats.CacheDir, MsgLimit = i.Nats.MsgLimit, BytesLimit = i.Nats.BytesLimit, Amount = i.Nats.Amount, Bulk = i.Nats.Bulk, Interval = i.Nats.Interval, Version = i.Nats.Version, Created = i.Nats.Created, Deleted = i.Nats.Deleted });
                        result = JsonConvert.SerializeObject(items);
                        break;
                    default:
                        int i = data.Length;
                        if (i < 20) break;
                        if (data[0] == '{' && data[i - 1] == '}')
                        {
                            var item = new { act = "", data = new Subscribes() };
                            item = JsonConvert.DeserializeAnonymousType(data, item);
                            switch (item.act)
                            {
                                case "select":
                                    result = SelectItems(item.data);
                                    break;
                                case "insert":
                                    result = InsertItems(item.data);
                                    break;
                                case "update":
                                    result = UpdateItems(item.data);
                                    break;
                                case "delete":
                                    result = DeleteItems(item.data);
                                    break;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        #endregion

        /// <summary>
        /// 调用JS函数
        /// </summary>
        /// <param name="code">JS文本</param>
        /// <returns></returns>
        public object Invoke(string code) => JS.Engine.Invoke(JsFunction, JS.Engine.Evaluate(V8Script.JS.SecurityCode(code)));

        public object Clone() => JsonConvert.DeserializeObject<Config>(JsonConvert.SerializeObject(this));

        public Config Close()
        {
            natSubscription?.Unsubscribe();
            natsConnection?.Drain();
            return this;
        }

        Handler NewJsHandler() => new Handler(this, IsJsCron)
        {
            Action = (code, count) =>
            {
                lock (JS)
                {
                    try
                    {
                        var res = JS.Engine.Invoke(JsFunction, string.IsNullOrEmpty(code) ? DateTime.Now : JS.Engine.Evaluate(V8Script.JS.SecurityCode(code)), count);
                        if (!(res is Undefined))
                        {
                            // execute sql command
                            if ("String" == res.GetType().Name && res.ToString().Length >= 20)
                                res = JS.Database.x(res) + " records affected database";
                            Log.Information($"[{Subject}] <- @sql: {res}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"[{Subject}] <- @sql: error");
                    }
                }
            }
        };

        void DynamicLoad(object obj)
        {
            int loadInterval = Convert.ToInt32(obj), interval = loadInterval * 1000;
            while (true)
            {
                Thread.Sleep(interval);
                try
                {
                    // init js template
                    if (IsJsTemplate())
                    {
                        // script file content
                        string itemJs = Js;
                        if (!File.Exists(itemJs))
                        {
                            Nats.Deleted = true;
                            JsHandler.Stop();
                            continue;
                        }

                        var version = File.GetLastWriteTime(itemJs).ToFileTime();
                        if (Nats.Version != version)
                        {
                            // update version
                            Nats.Version = version;

                            var itemJsContent = File.ReadAllText(itemJs);
                            JsContent = itemJsContent;

                            var js = new V8Script.JS(itemJsContent, Db, Redis, JsHandler.Connection, Nats.Subscribe, Subject, true, false);
                            js.Add("config", MemberwiseClone());
                            js.Engine.Execute(js.Script);

                            if (js.Engine.Script.sql != null)
                            {
                                JS.Engine.Dispose();
                                JS.Database.Db.Dispose();
                                JS = js;
                            }

                            Log.Information($"[{Subject}] <- @update-version: {version}");
                        }

                        Nats.Deleted = false;
                        JsHandler.Start();
                    }

                    // init db template
                    if (IsDbTemplate())
                    {
                        if (JS.Database.Db == null) continue;

                        var v = JS.Database.Db.Ado.GetScalar($"Select Version From {Nats.Table} Where Deleted=0 and Name=@Name", new { Nats.Name });
                        if (v == null || v == DBNull.Value)
                        {
                            Nats.Deleted = true;
                            JsHandler.Stop();
                            continue;
                        }

                        var version = Convert.ToInt64(v);
                        if (Nats.Version != version)
                        {
                            // update version
                            Nats.Version = version;

                            var content = JS.Database.Db.Ado.GetScalar($"Select Content From {Nats.Table} Where Name=@Name", new { Nats.Name });
                            var itemJsContent = Convert.ToString(content);
                            JsContent = itemJsContent;

                            var js = new V8Script.JS(itemJsContent, Db, Redis, JsHandler.Connection, Nats.Subscribe, Subject, true, false);
                            js.Add("config", MemberwiseClone());
                            js.Engine.Execute(js.Script);

                            if (js.Engine.Script.sql != null)
                            {
                                JS.Engine.Dispose();
                                JS.Database.Db.Dispose();
                                JS = js;
                            }

                            Log.Information($"[{Subject}] <- @update-version: {version}");
                        }

                        Nats.Deleted = false;
                        JsHandler.Start();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"[{Subject}] <- @update-version: error");
                }
            }
        }
    }
}
