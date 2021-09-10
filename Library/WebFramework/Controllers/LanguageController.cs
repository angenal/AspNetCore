using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Net;
using WebCore;
using WebInterface;

namespace WebFramework.Controllers
{
    /// <summary>
    /// 语言
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LanguageController : ApiController
    {
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration config;
        private readonly IMemoryCache cache;

        /// <summary></summary>
        public LanguageController(IWebHostEnvironment env, IConfiguration config, IMemoryCache cache)
        {
            this.env = env;
            this.config = config;
            this.cache = cache;
        }

        /// <summary>
        /// 代码列表
        /// </summary>
        /// <param name="fieldAsKey">"零":由值作为返回对象属性; "非零":由名称作为对象属性</param>
        [HttpGet("{fieldAsKey=0}")]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult Locales(string fieldAsKey)
        {
            object result = fieldAsKey == "0" ? typeof(Language).ToDescriptions() : typeof(Language).ToDescriptionsFieldAsKey();
            return Ok(result);
        }
    }
}
