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
            DateTime date = Date.Now(), dateTime = DateTime.Now;

            return Ok(new
            {
                Date = new
                {
                    Now = date.ToDateTimeString(),
                    Json = $"new Date({date.ToJavaScriptTicks()})",
                    date.Kind,
                    Zone = "Asia/Shanghai"
                },
                DateTime = new
                {
                    Now = dateTime.ToDateTimeString(),
                    Json = $"new Date({dateTime.ToJavaScriptTicks()})",
                    dateTime.Kind
                }
            });
        }
    }
}
