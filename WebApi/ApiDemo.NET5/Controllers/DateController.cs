using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using WebCore;
using WebFramework;

namespace ApiDemo.NET5.Controllers
{
    /// <summary>
    /// 日期时间
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DateController : ApiController
    {
        private readonly IWebHostEnvironment env;

        /// <summary>
        ///
        /// </summary>
        public DateController(IWebHostEnvironment env)
        {
            this.env = env;
        }

        /// <summary>
        /// Returns current time info.
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
                Startup = Date.Startup.ToDateTimeString(),
                env.ApplicationName,
                env.EnvironmentName,
                Date.Tzdb
            });
        }
    }
}
