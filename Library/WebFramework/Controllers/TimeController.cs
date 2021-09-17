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
                //WebCore.Platform.PlatformDetails.HostName,
                WebCore.Platform.PlatformDetails.MachineName,
                RunningOn = WebCore.Platform.PlatformDetails.RunningOn(),
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
        /// 检测消息推送(每分钟)
        /// </summary>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> RecurringPushMessagesEveryMinute()
        {
            var id = crypto.Md5(env.ApplicationName + nameof(RecurringPushMessagesEveryMinute));
            RecurringJob.AddOrUpdate(id, () => new TimeJob(hubContext).Execute(), Cron.Minutely());

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
        public TimeJob(IMemoryCache cache) => this.cache = cache;
        /// <summary></summary>
        public TimeJob(IHubContext<ChatHub> hubContext) => this.hubContext = hubContext;
        /// <summary></summary>
        public TimeJob(IMemoryCache cache, IHubContext<ChatHub> hubContext)
        {
            this.cache = cache;
            this.hubContext = hubContext;
        }

        #endregion

        /// <summary>执行消息推送</summary>
        public void Execute()
        {
            hubContext.Clients.All.SendAsync("newMessage", $"新消息 {DateTime.Now.ToTimeString()}").Wait();
        }
    }
}
