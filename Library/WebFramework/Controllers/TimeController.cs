using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using SqlSugar;
using System;
using System.Net;
using System.Threading.Tasks;
using WebCore;
using WebFramework.Data;
using WebFramework.SignalR;
using WebInterface;

namespace WebFramework.Controllers
{
    /// <summary>
    /// 系统时间
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TimeController : ApiController
    {
        private readonly IWebHostEnvironment env;
        private readonly IMemoryCache cache;
        private readonly ICrypto crypto;
        private readonly IHubContext<ChatHub> hubContext;

        /// <summary></summary>
        public TimeController(IWebHostEnvironment env, IMemoryCache cache, ICrypto crypto, IHubContext<ChatHub> hubContext)
        {
            this.env = env;
            this.cache = cache;
            this.crypto = crypto;
            this.hubContext = hubContext;
        }

        /// <summary>
        /// 当前时间
        /// </summary>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult Now()
        {
            string timeZone = Date.DefaultTimeZone.Id;
            DateTime date = Date.Now(timeZone), dateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local);

            return Ok(new
            {
                env.ApplicationName,
                env.EnvironmentName,
                Startup = Date.Startup.ToDateTimeString(),
                Uptime = DateTime.Now - Date.Startup,
                Linux = new
                {
                    Now = date.ToDateTimeString(),
                    Json = $"new Date({date.ToJavaScriptTicks()})",
                    Kind = date.Kind.ToString(),
                    Local = Date.DefaultTimeZone
                },
                Windows = new
                {
                    Now = dateTime.ToDateTimeString(),
                    Json = $"new Date({dateTime.ToJavaScriptTicks()})",
                    Kind = dateTime.Kind.ToString(),
                    TimeZoneInfo.Local
                },
                Date.Tzdb
            });
        }

        /// <summary>
        /// Schedule TimeJob Input
        /// </summary>
        public class ScheduleNowInputDto
        {
            /// <summary>
            /// Delay time, e.g. 00:01:00
            /// </summary>
            public string Delay { get; set; }
        }

        /// <summary>
        /// Schedule TimeJob after delay time
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> ScheduleNow(ScheduleNowInputDto input)
        {
            if (!TimeSpanParser.TryParse(input.Delay, out TimeSpan delay) || delay < TimeSpan.FromSeconds(5))
                return Error("参数错误！");

            var cacheKey = crypto.Md5(env.ApplicationName + crypto.RandomString(8));
            cache.Set(cacheKey, DateTime.Now);

            var id = BackgroundJob.Schedule(() => new TimeJob(cache, hubContext).Execute(cacheKey, delay), delay);

            return Ok(await Task.FromResult(id));
        }
    }

    /// <summary></summary>
    public class TimeJob
    {
        #region constructor

        private readonly IMemoryCache cache;
        private readonly IHubContext<ChatHub> hubContext;

        /// <summary>
        /// new SqlSugarClient
        /// </summary>
        public SqlSugarClient db => _db ??= SQLServerDb.DefaultConnection.NewSqlSugarClient();
        private SqlSugarClient _db;

        /// <summary></summary>
        public TimeJob() { }
        /// <summary></summary>
        public TimeJob(SqlSugarClient db) => _db = db;
        /// <summary></summary>
        public TimeJob(IMemoryCache cache, IHubContext<ChatHub> hubContext)
        {
            this.cache = cache;
            this.hubContext = hubContext;
        }

        #endregion

        /// <summary></summary>
        public void Execute(string cacheKey, TimeSpan schedule)
        {
            DateTime now = DateTime.Now, cacheTime = cache.Get<DateTime>(cacheKey), time = cacheTime.Add(schedule);
            // error is within 5 seconds
            if (now.ToTimestampSeconds() - time.ToTimestampSeconds() < 5)
            {
                System.Diagnostics.Debug.WriteLine($"{nameof(TimeJob)} executes successfully.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"{nameof(TimeJob)} executes failed.");
            }
        }
    }
}
