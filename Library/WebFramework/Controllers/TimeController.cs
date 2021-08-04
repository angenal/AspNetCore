using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using WebCore;

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

        /// <summary>
        ///
        /// </summary>
        public TimeController(IWebHostEnvironment env)
        {
            this.env = env;
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
    }
}
