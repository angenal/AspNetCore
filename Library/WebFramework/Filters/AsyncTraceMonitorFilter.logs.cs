using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WebCore;
using WebFramework.Data;
using WebFramework.Services;

namespace WebFramework.Filters
{
    /// <summary>
    /// Request log database model
    /// </summary>
    public class RequestLog
    {
        /// <summary>编号</summary>
        [Display(Name = "编号")]
        [Key()]
        public Guid Id { get; set; }
        /// <summary>跟踪网址</summary>
        [Display(Name = "跟踪网址")]
        public string Path { get; set; }
        /// <summary>跟踪编号</summary>
        [Display(Name = "跟踪编号")]
        public string Trace { get; set; }
        /// <summary>请求内容</summary>
        [Display(Name = "请求内容")]
        public string Request { get; set; }
        /// <summary>响应内容</summary>
        [Display(Name = "响应内容")]
        public string Response { get; set; }
        /// <summary>产生时间</summary>
        [Display(Name = "产生时间")]
        public DateTime Time { get; set; }
    }
    /// <summary>
    /// Request log output
    /// </summary>
    public class RequestLogOutputDto
    {
        /// <summary></summary>
        public int Records { get; set; }
        /// <summary></summary>
        public int Page { get; set; }
        /// <summary></summary>
        public int Total { get; set; }
        /// <summary></summary>
        public List<RequestLog> Rows { get; set; }
    }

    /// <summary>
    /// Request log database service
    /// </summary>
    public class RequestLogService
    {
        /// <summary>
        /// Use LiteDb
        /// </summary>
        internal static LiteDb LogDb;

        /// <summary>
        /// Init LiteDb
        /// </summary>
        /// <param name="path"></param>
        internal static void Init(string path)
        {
            path = Path.Combine(path, "200.db");
            var exists = File.Exists(path);
            LogDb = new LiteDb(path, false);
            if (exists) return;
            using var db = LogDb.Open();
            var c = db.GetCollection<RequestLog>(LiteDB.BsonAutoId.Guid);
            c.EnsureIndex(t => t.Path);
            c.EnsureIndex(t => t.Trace);
            //c.EnsureIndex(t => t.Time);
        }

        /// <summary>
        /// Record log, if exists web logs/500 directory, and write LiteDb
        /// </summary>
        internal static void WriteLog(RequestLog log)
        {
            try
            {
                //// Record log file
                //var path = Path.Combine(StatusDir500, $"{log.Trace}.txt");
                //if (File.Exists(path)) File.AppendAllText(path, log.Content);
                //else File.WriteAllText(path, log.Content);
                // Record log database
                using var db = LogDb.Open();
                var c = db.GetCollection<RequestLog>(LiteDB.BsonAutoId.Guid);
                c.Insert(log);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Config log records
        /// </summary>
        public const string ConfigUrl = "/api/log/request/trace/{id?}";

        /// <summary>
        /// Config log records from LiteDb
        /// </summary>
        public static async Task ConfigHandler(HttpContext context)
        {
            string trace = context.Request.RouteValues["id"]?.ToString();
            if (!string.IsNullOrEmpty(trace)) Logs.Manage.Trace = (trace != "0" && trace != "no" && trace != "false");
            string text = "{\"trace\":" + Logs.Manage.Trace.ToString().ToLower() + "}";
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(text);
        }

        /// <summary>
        /// Query log records Url
        /// </summary>
        public const string QueryUrl = "/api/log/request/query/{page}";

        /// <summary>
        /// Query log records from LiteDb
        /// </summary>
        public static async Task QueryHandler(HttpContext context)
        {
            var req = context.Request;
            string text = string.Empty, search = req.Query["search"].ToString(), callback = req.Query["callback"].ToString(), page = req.RouteValues["page"].ToString();
            if (!int.TryParse(page, out int pageIndex)) pageIndex = 0;
            int pageSize = 20;
            using (var db = LogDb.Open())
            {
                var q = db.GetCollection<RequestLog>(LiteDB.BsonAutoId.Guid).Query();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    q = Guid.TryParse(search, out _) ? q.Where(t => t.Trace == search) : q.Where(t => t.Path.Contains(search));
                }
                if (pageIndex <= 0)
                {
                    var result = q.OrderByDescending(t => t.Time).ToList();
                    text = result.ToJson();
                }
                else
                {
                    var result = new RequestLogOutputDto()
                    {
                        Page = pageIndex,
                        Records = q.Count(),
                    };
                    result.Total = (int)Math.Ceiling((double)result.Records / pageSize);
                    result.Rows = q.OrderByDescending(t => t.Time).Skip(pageSize * (pageIndex - 1)).Limit(pageSize).ToList();
                    text = result.ToJson();
                }
            }
            bool jsonp = !string.IsNullOrEmpty(callback);
            context.Response.ContentType = jsonp ? "text/x-json" : "application/json";
            if (jsonp) text = $"{HttpUtility.UrlEncode(callback)}({text})";
            await context.Response.WriteAsync(text);
        }

        /// <summary>
        /// Delete log records Url
        /// </summary>
        public const string DeleteUrl = "/api/log/request/delete/{id}";

        /// <summary>
        /// Delete log records from LiteDb
        /// </summary>
        public static async Task DeleteHandler(HttpContext context)
        {
            string text = "{\"deleted\":0}", id = context.Request.RouteValues["id"].ToString();
            if (Guid.TryParse(id, out Guid guid))
            {
                using (var db = LogDb.Open())
                {
                    var c = db.GetCollection<RequestLog>(LiteDB.BsonAutoId.Guid);
                    var ok = c.Delete(new LiteDB.BsonValue(guid));
                    text = "{\"deleted\":" + ok.ToString().ToLower() + "}";
                }
            }
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(text);
        }
    }

    /// <summary>
    /// Asynchronous subscription publication > 3k Requests/sec + The background processing time is about one minute.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncRequestHandler<T> : IDisposable
    {
        /// <summary>
        /// Subscribe Tasks
        /// </summary>
        internal List<Delegate> Handlers = new List<Delegate>();

        /// <summary>
        /// data list
        /// </summary>
        private readonly Dictionary<long, ConcurrentQueue<T>> _data = new Dictionary<long, ConcurrentQueue<T>>();

        /// <summary>
        /// parallel tasks number
        /// </summary>
        private readonly int _onceConcurrentTasks;
        /// <summary>
        /// sleep 1 milliseconds after parallel tasks, for load reduction
        /// </summary>
        private readonly TimeSpan _onceInterval;
        /// <summary>
        /// sleep 1 seconds before parallel tasks, for load reduction
        /// </summary>
        private readonly TimeSpan _interval;
        /// <summary>
        /// a task timeout
        /// </summary>
        private readonly TimeSpan _timeout;
        /// <summary>
        /// a background thread
        /// </summary>
        private readonly Thread _thread0;
        private readonly Thread _thread1;
        private bool _started;
        private long _timeStamp;
        private long _timeStampSeed;

        /// <summary></summary>
        private static AsyncRequestHandler<T> _default;

        /// <summary></summary>
        public static AsyncRequestHandler<T> Default => _default ??= new AsyncRequestHandler<T>(TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30)).Start();

        /// <summary></summary>
        public AsyncRequestHandler(TimeSpan onceInterval, TimeSpan interval, TimeSpan timeout, int onceConcurrentTasks = 0)
        {
            _onceConcurrentTasks = onceConcurrentTasks <= 0 ? Environment.ProcessorCount : onceConcurrentTasks;
            _onceInterval = onceInterval;
            _interval = interval;
            _timeout = timeout;
            _timeStamp = _timeStampSeed = DateTimeOffset.Now.ToUnixTimeSeconds();
            _data.Add(_timeStamp, new ConcurrentQueue<T>());
            _thread0 = new Thread(RunSubscribeTasks) { IsBackground = true };
            _thread1 = new Thread(RunPublishTasks) { IsBackground = true };
        }

        /// <summary>
        /// Publish Tasks
        /// </summary>
        /// <param name="item"></param>
        public void Publish(T item)
        {
            if (!_started) return;
            _data[_timeStamp].Enqueue(item);
        }

        /// <summary>
        /// Subscribe Tasks
        /// </summary>
        /// <param name="handler"></param>
        public void Subscribe(Action<T> handler)
        {
            if (!_started) return;
            Handlers.Add(handler);
        }

        /// <summary>
        /// Subscribe Tasks
        /// </summary>
        /// <param name="handler"></param>
        public void Subscribe(Func<T, Task> handler)
        {
            if (!_started) return;
            Handlers.Add(handler);
        }

        /// <summary></summary>
        public void Unsubscribe()
        {
            Handlers.Clear();
        }

        /// <summary></summary>
        public AsyncRequestHandler<T> Start()
        {
            if (_started) return this;
            try
            {
                _thread0.Start();
                _thread1.Start();
                _started = true;
            }
            catch (Exception)
            {
                // ignored
            }
            return this;
        }

        /// <summary></summary>
        public void Dispose()
        {
            Unsubscribe();
        }

        /// <summary></summary>
        internal void RunPublishTasks()
        {
            var isInterval = _interval != TimeSpan.Zero && _interval.TotalMilliseconds > 0;
            var millisecondsTimeout = isInterval ? (int)_interval.TotalMilliseconds : 1000;
            while (true)
            {
                lock (_data) _data.Add(_timeStamp + 1, new ConcurrentQueue<T>());
                Thread.Sleep(millisecondsTimeout);
                Interlocked.Increment(ref _timeStamp);
            }
        }

        /// <summary></summary>
        internal void RunSubscribeTasks()
        {
            var isInterval = _interval != TimeSpan.Zero && _interval.TotalMilliseconds > 0;
            var isOnceInterval = _onceInterval != TimeSpan.Zero && _onceInterval.TotalMilliseconds > 0;
            while (_started)
            {
                if (isInterval) Thread.Sleep(_interval); // sleep 1 seconds before parallel tasks, for load reduction
                try
                {
                    while (Handlers.Count > 0 && _timeStampSeed < _timeStamp)
                    {
                        var items = _data[_timeStampSeed];
                        var hasValue = items.TryPeek(out _);
                        while (hasValue)
                        {
                            var list = new List<T>();
                            for (var i = 0; i < _onceConcurrentTasks && items.TryDequeue(out var item); i++) list.Add(item);
                            if (list.Count == 1)
                            {
                                var item = list[0];
                                try
                                {
                                    foreach (var handler in Handlers)
                                    {
                                        switch (handler)
                                        {
                                            case Action<T> action:
                                                Run(action, item).ConfigureAwait(false).GetAwaiter().GetResult();
                                                break;
                                            case Func<T, Task> func:
                                                Run(func, item).ConfigureAwait(false).GetAwaiter().GetResult();
                                                break;
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    Publish(item); // rollback after Request
                                }
                            }
                            else
                            {
                                Parallel.ForEach(list, item =>
                                {
                                    try
                                    {
                                        foreach (var handler in Handlers)
                                        {
                                            switch (handler)
                                            {
                                                case Action<T> action:
                                                    Run(action, item).ConfigureAwait(false).GetAwaiter().GetResult();
                                                    break;
                                                case Func<T, Task> func:
                                                    Run(func, item).ConfigureAwait(false).GetAwaiter().GetResult();
                                                    break;
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        Publish(item); // rollback after Request
                                    }
                                });
                            }
                            if (isOnceInterval) Thread.Sleep(_onceInterval); // sleep 1 milliseconds after parallel tasks, for load reduction
                            hasValue = items.TryPeek(out _);
                        }
                        lock (_data) _data.Remove(_timeStampSeed);
                        _timeStampSeed++;
                    }
                }
                catch (Exception) { }
            }
        }

        /// <summary></summary>
        internal Task Run(Action<T> action, T data)
        {
            var task1 = Task.Delay(_timeout);
            var task2 = Task.Factory.StartNew(() => action(data));
            return Task.WhenAny(task1, task2);
        }

        /// <summary></summary>
        internal Task Run(Func<T, Task> func, T data)
        {
            var task1 = Task.Delay(_timeout);
            var task2 = func(data);
            return Task.WhenAny(task1, task2);
        }

        /// <summary></summary>
        internal Task Run(Action<T[]> action, T[] data)
        {
            var task1 = Task.Delay(_timeout);
            var task2 = Task.Factory.StartNew(() => action(data));
            return Task.WhenAny(task1, task2);
        }

        /// <summary></summary>
        internal Task Run(Func<T[], Task> func, T[] data)
        {
            var task1 = Task.Delay(_timeout);
            var task2 = func(data);
            return Task.WhenAny(task1, task2);
        }
    }
}
