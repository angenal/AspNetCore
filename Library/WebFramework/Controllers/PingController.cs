using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using WebCore;
using WebCore.Platform;

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
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult Status()
        {
            return Ok(new
            {
                OS = OS.Name,
                OS.Version,
                Environment.MachineName,
                env.ApplicationName,
                env.EnvironmentName,
                Startup = Date.Startup.ToDateTimeString(),
                Uptime = DateTime.Now - Date.Startup,
            });
        }
    }
}
