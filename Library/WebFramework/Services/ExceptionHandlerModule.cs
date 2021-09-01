using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WebCore;
using WebFramework.Data;

namespace WebFramework.Services
{
    /* appsettings.json
      "Logging": {
        "LogLevel": { },
        "LogManage": {
          "Path": "logs",
          "User": "demo",
          "Pass": "demo"
        }
      },
    */

    /// <summary>
    /// Configure Global monitoring and exception handler module
    /// </summary>
    public static class ExceptionHandlerModule
    {
        //static readonly BackgroundJobClient JobClient = new BackgroundJobClient(new MemoryStorage(new MemoryStorageOptions()));
        //static readonly BackgroundJobServer JobServer = new BackgroundJobServer(new BackgroundJobServerOptions { ServerName = $"{LogsRootDir}-{Environment.ProcessId}" }, new MemoryStorage(new MemoryStorageOptions()));

        /// <summary>
        /// Configuration Section in appsettings.json
        /// </summary>
        const string AppSettings = "Logging:LogManage";
        /// <summary>
        /// Web logs root directory
        /// </summary>
        public static string LogsRootDir = "logs";
        /// <summary>
        /// Web logs record cache enabled
        /// </summary>
        public static bool CacheEnabled = false;
        /// <summary>
        /// Web logs directory for status 500
        /// </summary>
        static string StatusDir500 = StatusCodes.Status500InternalServerError.ToString();
        /// <summary>
        /// Web logs record status 500
        /// </summary>
        static bool StatusDir500Exists = false;
        /// <summary>
        /// Asynchronous record log file
        /// </summary>
        static AsyncExceptionHandler<ExceptionLog> LogHandler;

        /// <summary>
        /// Init Exception Module
        /// </summary>
        public static void Init(IConfiguration config, IWebHostEnvironment env)
        {
            var section = config.GetSection(AppSettings);
            if (!section.Exists()) return;
            if (section.GetSection("Path").Exists()) LogsRootDir = section.GetValue<string>("Path").Trim('/');
            var path = Path.Combine(env.WebRootPath, LogsRootDir);
            if (!Directory.Exists(path)) return;
            ExceptionLogService.Init(path);
            CacheEnabled = true;
            StatusDir500 = Path.Combine(path, StatusDir500);
            StatusDir500Exists = Directory.Exists(StatusDir500);
            LogHandler = new AsyncExceptionHandler<ExceptionLog>(TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), 1).Start();
            LogHandler.Subscribe(ExceptionLogService.WriteLog);
        }

        /// <summary>
        /// Global Error Handler for Status 400 BadRequest with Invalid ModelState
        /// </summary>
        public static void ApiBehavior(ApiBehaviorOptions options)
        {
            //options.ClientErrorMapping[StatusCodes.Status404NotFound].Link = "https://*.com/404";
            //options.SuppressConsumesConstraintForFormFileParameters = true;
            //options.SuppressInferBindingSourcesForParameters = true;
            //options.SuppressModelStateInvalidFilter = true; // 关闭系统自带模型验证(使用第三方库FluentValidation)
            //options.SuppressMapClientErrors = true;
            options.InvalidModelStateResponseFactory = context =>
            {
                var s = context.ModelState.Values;
                if (context.ModelState.IsValid || !s.Any(i => i.Errors.Any()))
                    return new OkObjectResult(context.ModelState);
                var x = s.Where(i => i.Errors.Any());
                var result = new BadRequestObjectResult(new
                {
                    status = 400,
                    title = x.First().Errors.First().ErrorMessage.Replace("＆", " "),
                    errors = string.Join("；", x.Select(v => string.Join("；", v.Errors.Select(e => e.ErrorMessage)))).Replace("＆", " ")
                });
                result.ContentTypes.Add(System.Net.Mime.MediaTypeNames.Application.Json);
                return result;
            };
        }

        /// <summary>
        /// Global Exception Handler for Status 404 ~ 500 Internal Server Error
        /// </summary>
        /// <param name="options"></param>
        public static void ExceptionHandler(ExceptionHandlerOptions options)
        {
            // Passing by 404
            options.AllowStatusCode404Response = true;
            // The path to the exception handling endpoint for MVC
            //options.ExceptionHandlingPath = new PathString("/error");
            // Handle the exception
            options.ExceptionHandler = context =>
            {
                var e = context.Features.Get<IExceptionHandlerFeature>().Error;
                if (e is WebException)
                {
                    context.Abort();
                    return Task.CompletedTask;
                }

                var text = string.Empty;
                if (e is TaskCanceledException || e is OperationCanceledException)
                {
                    context.Response.StatusCode = StatusCodes.Status204NoContent;
                    return context.Response.WriteAsync(text);
                }

                // Check if it is an API request
                const int status = StatusCodes.Status500InternalServerError;
                if (!context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = status;
                    return context.Response.WriteAsync(e.Message);
                }

                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = status;

                var trace = context.TraceIdentifier;
                var url = context.Request.GetDisplayUrl();
                string detail = e.ToString(), details = detail;
                string[] s = detail.Split(Environment.NewLine);
                if (s.Length > 3) detail = string.Join(" ↓", s[0], s[1], s[2]);
                var error = new { title = e.Message, detail, trace, status };

                // Record logs, if exists web logs/500 directory
                //if (!StatusDir500Exists) Serilog.Log.Logger.Error(e, url);
                var contents = new StringBuilder(url);
                contents.Append(Environment.NewLine);
                contents.Append(Environment.NewLine);

                // Gets trace request contents
                if (trace != null && context.Items.TryGetValue(trace, out object value) && value != null)
                {
                    if (value is string body)
                    {
                        contents.Append($" body => ");
                        contents.Append(string.IsNullOrWhiteSpace(body) ? "null" : body);
                        contents.Append(Environment.NewLine);
                    }
                    else if (value is IDictionary<string, object> input)
                    {
                        foreach (var key in input.Keys)
                        {
                            contents.Append($" {key} => ");
                            contents.Append(input[key]?.ToJson() ?? "null");
                            contents.Append(Environment.NewLine);
                        }
                    }
                }

                // Write origin error logs
                contents.Append(Environment.NewLine);
                contents.Append(details);

                // Asynchronous record log file
                LogHandler.Publish(new ExceptionLog
                {
                    Path = context.Request.Path.Value,
                    Trace = error.trace,
                    Message = e.Message,
                    Content = contents.ToString(),
                    Time = DateTime.Now,
                });

                text = Newtonsoft.Json.JsonConvert.SerializeObject(error);

                return context.Response.WriteAsync(text);
            };
        }


        /// <summary>
        /// Global Monitor System
        /// Sentry: Exception Monitoring Platform
        /// https://sentry.io/signup
        /// https://github.com/docker-library/docs/tree/master/sentry
        /// https://github.com/getsentry/onpremise > ./install.sh (docker)
        /// </summary>
        public static IWebHostBuilder UseSentryMonitor(this IWebHostBuilder builder)
        {
            //var section = config.GetSection(SentrySettings.AppSettings);
            //if (!section.Exists()) return services;

            //// Register IOptions<SentrySettings> from appsettings.json
            //services.Configure<SentrySettings>(section);
            //config.Bind(SentrySettings.AppSettings, SentrySettings.Instance);

            //builder.UseSentry((context, options) =>
            //{
            //    options.Environment = context.HostingEnvironment.EnvironmentName;
            //    options.Dsn = context.Configuration.GetSection("Sentry:Dsn").Value;
            //    options.Debug = bool.Parse(context.Configuration.GetSection("Sentry:Debug").Value);
            //    options.SendDefaultPii = bool.Parse(context.Configuration.GetSection("Sentry:SendDefaultPii").Value);
            //    options.AttachStacktrace = bool.Parse(context.Configuration.GetSection("Sentry:AttachStacktrace").Value);
            //    options.MaxRequestBodySize = Enum.Parse<Sentry.Extensibility.RequestSize>(context.Configuration.GetSection("Sentry:MaxRequestBodySize").Value);
            //    options.MinimumBreadcrumbLevel = Enum.Parse<Microsoft.Extensions.Logging.LogLevel>(context.Configuration.GetSection("Sentry:MinimumBreadcrumbLevel").Value);
            //    options.MinimumEventLevel = Enum.Parse<Microsoft.Extensions.Logging.LogLevel>(context.Configuration.GetSection("Sentry:MinimumEventLevel").Value);
            //    options.DiagnosticLevel = Enum.Parse<Sentry.SentryLevel>(context.Configuration.GetSection("Sentry:DiagnosticLevel").Value);
            //    options.BeforeSend = (e) => e.Exception is TaskCanceledException || e.Exception is OperationCanceledException ? null : e;
            //});

            return builder;
        }
    }

    /// <summary>
    /// Exception log database model
    /// </summary>
    public class ExceptionLog
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
        /// <summary>异常消息</summary>
        [Display(Name = "异常消息")]
        public string Message { get; set; }
        /// <summary>异常详情</summary>
        [Display(Name = "异常详情")]
        public string Content { get; set; }
        /// <summary>产生时间</summary>
        [Display(Name = "产生时间")]
        public DateTime Time { get; set; }
    }
    /// <summary>
    /// Exception log output
    /// </summary>
    public class ExceptionLogOutputDto
    {
        /// <summary></summary>
        public int Records { get; set; }
        /// <summary></summary>
        public int Page { get; set; }
        /// <summary></summary>
        public int Total { get; set; }
        /// <summary></summary>
        public List<ExceptionLog> Rows { get; set; }
    }

    /// <summary>
    /// Exception log database service
    /// </summary>
    public class ExceptionLogService
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
            path = Path.Combine(path, "index.db");
            var exists = File.Exists(path);
            LogDb = new LiteDb(path, false);
            if (exists) return;
            using var db = LogDb.Open();
            var c = db.GetCollection<ExceptionLog>(LiteDB.BsonAutoId.Guid);
            c.EnsureIndex(t => t.Path);
            c.EnsureIndex(t => t.Trace);
            //c.EnsureIndex(t => t.Time);
        }

        /// <summary>
        /// Record log, if exists web logs/500 directory, and write LiteDb
        /// </summary>
        internal static void WriteLog(ExceptionLog log)
        {
            try
            {
                //// Record log file
                //var path = Path.Combine(StatusDir500, $"{log.Trace}.txt");
                //if (File.Exists(path)) File.AppendAllText(path, log.Content);
                //else File.WriteAllText(path, log.Content);
                // Record log database
                using var db = LogDb.Open();
                var c = db.GetCollection<ExceptionLog>(LiteDB.BsonAutoId.Guid);
                c.Insert(log);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Query log records Url
        /// </summary>
        public const string QueryUrl = "/api/log/exception/query/{page}";

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
                var q = db.GetCollection<ExceptionLog>(LiteDB.BsonAutoId.Guid).Query();
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
                    var result = new ExceptionLogOutputDto()
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
        public const string DeleteUrl = "/api/log/exception/delete/{id}";

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
                    var c = db.GetCollection<ExceptionLog>(LiteDB.BsonAutoId.Guid);
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
    internal class AsyncExceptionHandler<T> : IDisposable
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
        private static AsyncExceptionHandler<T> _default;

        /// <summary></summary>
        public static AsyncExceptionHandler<T> Default => _default ??= new AsyncExceptionHandler<T>(TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30)).Start();

        /// <summary></summary>
        public AsyncExceptionHandler(TimeSpan onceInterval, TimeSpan interval, TimeSpan timeout, int onceConcurrentTasks = 0)
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
        public AsyncExceptionHandler<T> Start()
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
                                    Publish(item); // rollback after exception
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
                                        Publish(item); // rollback after exception
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
