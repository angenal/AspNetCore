using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using WebCore;

namespace WebFramework.Controllers
{
    /// <summary>
    /// 系统检查
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PingController : ApiController
    {
        private readonly IWebHostEnvironment env;

        /// <summary></summary>
        public PingController(IWebHostEnvironment env)
        {
            this.env = env;
        }

        /// <summary>
        /// 当前状态
        /// </summary>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult Status()
        {
            return Ok(new
            {
                WebCore.Platform.PlatformDetails.HostName,
                WebCore.Platform.PlatformDetails.MachineName,
                RunningOn = WebCore.Platform.PlatformDetails.RunningOn(),
                env.ApplicationName,
                env.EnvironmentName,
                Startup = Date.Startup.ToDateTimeString(),
                Uptime = DateTime.Now - Date.Startup,
            });
        }
    }
}
