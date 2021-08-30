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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebCore;

namespace WebFramework.Services
{
    /// <summary>
    /// Configure Global monitoring and exception handler module
    /// </summary>
    public static class ExceptionHandlerModule
    {
        //static readonly BackgroundJobClient JobClient = new BackgroundJobClient(new MemoryStorage());
        //static readonly BackgroundJobServer JobServer = new BackgroundJobServer(new BackgroundJobServerOptions { ServerName = $"{LogsRootDir}-{Environment.ProcessId}" }, new MemoryStorage(new MemoryStorageOptions()));

        /// <summary>
        /// Web logs root directory
        /// </summary>
        public const string LogsRootDir = "logs";
        /// <summary>
        /// Asynchronous record log file
        /// </summary>
        static AsyncExceptionHandler<AsyncExceptionLogfileModel> LogsHandler;
        /// <summary>
        /// Web logs directory for status 500
        /// </summary>
        static string StatusDir500 = StatusCodes.Status500InternalServerError.ToString();
        /// <summary>
        /// Web logs record status 500
        /// </summary>
        static bool StatusDir500Exists = false;
        /// <summary>
        /// Web logs record cache enabled
        /// </summary>
        public static bool CacheEnabled = false;

        /// <summary>
        /// Init Exception Module
        /// </summary>
        public static void Init(IConfiguration config, IWebHostEnvironment env)
        {
            var path = Path.Combine(env.WebRootPath, LogsRootDir);
            if (!Directory.Exists(path)) return;
            LogsHandler = AsyncExceptionHandler<AsyncExceptionLogfileModel>.Default;
            StatusDir500 = Path.Combine(path, StatusDir500);
            StatusDir500Exists = Directory.Exists(StatusDir500);
            if (StatusDir500Exists) CacheEnabled = true;
        }

        /// <summary>
        /// Global Error Handler for Status 400 BadRequest with Invalid ModelState
        /// </summary>
        public static void ApiBehavior(ApiBehaviorOptions options)
        {
            //options.ClientErrorMapping[StatusCodes.Status404NotFound].Link = "https://*.com/404";
            //options.SuppressConsumesConstraintForFormFileParameters = true;
            //options.SuppressInferBindingSourcesForParameters = true;
            //options.SuppressModelStateInvalidFilter = true; // 关闭系统自带模型验证(使用第三方库代理)
            //options.SuppressMapClientErrors = true;
            options.InvalidModelStateResponseFactory = context =>
            {
                var s = context.ModelState.Values;
                if (context.ModelState.IsValid || !s.Any() || !s.First().Errors.Any())
                    return new OkObjectResult(context.ModelState);
                var result = new BadRequestObjectResult(new
                {
                    status = 400,
                    title = s.First().Errors.First().ErrorMessage.Replace("＆", " "),
                    errors = string.Join("；", s.Select(v => string.Join("；", v.Errors.Select(e => e.ErrorMessage)))).Replace("＆", " ")
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

                // Record logs, if exists web logs directory
                if (StatusDir500Exists)
                {
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
                    LogsHandler.Publish(new AsyncExceptionLogfileModel
                    {
                        Path = Path.Combine(StatusDir500, $"{error.trace}.txt"),
                        Content = contents.ToString(),
                    });
                }
                else
                {
                    Serilog.Log.Logger.Error(e, url);
                }

                text = Newtonsoft.Json.JsonConvert.SerializeObject(error);

                return context.Response.WriteAsync(text);
            };
        }

        /// <summary>
        /// Record log file
        /// </summary>
        static void RecordLog(string path, string contents)
        {
            if (File.Exists(path)) File.AppendAllText(path, contents);
            else File.WriteAllText(path, contents);
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
    /// Asynchronous record log file
    /// </summary>
    internal class AsyncExceptionLogfileModel
    {
        /// <summary></summary>
        public string Path { get; set; }
        /// <summary></summary>
        public string Content { get; set; }
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
                            for (var i = 0; i < _onceConcurrentTasks && items.TryDequeue(out var item); i++)
                                list.Add(item);
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
