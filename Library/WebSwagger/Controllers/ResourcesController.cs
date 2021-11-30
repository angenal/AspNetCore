using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebSwagger.Internals;

namespace WebSwagger.Controllers
{
    /// <summary>
    /// Resources 控制器
    /// </summary>
    [AllowAnonymous]
    [Route("swagger/resources")]
    public class ResourcesController : Controller
    {
        /// <summary>
        /// 获取资源。通过 /swagger/resources?name=
        /// </summary>
        /// <param name="name">资源文件名</param>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ContentResult> GetAsync([FromQuery] string name)
        {
            var result = new ContentResult();
            var names = name.Split('.');
            if (names.Length < 2) return result;
            var suffix = names[names.Length - 1];
            result.Content = await Common.LoadContentAsync(name);
            result.ContentType = suffix.Equals("css", StringComparison.OrdinalIgnoreCase) ? "text/css"
                : suffix.Equals("js", StringComparison.OrdinalIgnoreCase) ? "application/javascript"
                : "text/plain";
            return result;
        }

        /// <summary>
        /// 获取语言资源文件
        /// </summary>
        /// <param name="name">语言名称</param>
        [HttpGet("getLanguage")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ContentResult> GetLanguageAsync([FromQuery] string name)
        {
            var result = new ContentResult
            {
                Content = await Common.GetLanguageAsync(name),
                ContentType = "application/javascript"
            };
            return result;
        }
    }
}
