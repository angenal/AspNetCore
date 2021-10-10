using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using WebCore;
using WebFramework;
using WebFramework.Services;
using WebInterface;

namespace WebControllers.Controllers
{
    /// <summary>
    /// 语言
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LanguageController : ApiController
    {
        //private readonly IWebHostEnvironment env;
        //private readonly IConfiguration config;
        //private readonly IMemoryCache cache;

        ///// <summary></summary>
        //public LanguageController(IWebHostEnvironment env, IConfiguration config, IMemoryCache cache)
        //{
        //    this.env = env;
        //    this.config = config;
        //    this.cache = cache;
        //}

        /// <summary>
        /// 默认值
        /// </summary>
        [HttpGet]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult Default()
        {
            return Ok(Localizations.DefaultCulture);
        }

        /// <summary>
        /// 可选项
        /// </summary>
        /// <param name="fieldAsKey">"零":由值作为返回对象属性; "非零":由名称作为对象属性</param>
        [HttpGet("{fieldAsKey=0}")]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult Options(string fieldAsKey)
        {
            object result = "0" == fieldAsKey ? typeof(Language).ToDescriptionsFieldAsKey() : typeof(Language).ToDescriptions();
            return Ok(result);
        }

        /// <summary>
        /// 修改默认值
        /// </summary>
        [HttpGet("{option=zh-CN}")]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult Update(string option)
        {
            if (!LanguageRouteConstraint.Cultures.Any(x => x.Equals(option, StringComparison.OrdinalIgnoreCase))
                && !LanguageRouteConstraint.Languages.Any(x => x.Equals(option, StringComparison.OrdinalIgnoreCase)))
                return Error("It's not an supported option.");

            var result = Localizations.SetDefaultCulture(option);
            return Ok(result);
        }
    }
}
