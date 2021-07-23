using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using UploadStream;

namespace WebFramework.Controllers
{
    /// <summary>
    /// 文件系统
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FileController : ApiController
    {
        private readonly IWebHostEnvironment env;

        /// <summary>
        ///
        /// </summary>
        public FileController(IWebHostEnvironment env)
        {
            this.env = env;
        }

        /// <summary>
        /// 上传
        /// </summary>
        [HttpPost("upload")]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Upload()
        {
            // returns a generic typed model, alternatively non-generic overload if no model binding is required
            var model = await this.StreamFiles<UploadModel>(async formFile =>
            {
                // implement processing of stream as required via an IFormFile interface
                using (var stream = formFile.OpenReadStream())
                {
                }
            });
            // ModelState is still validated from model
            //if (!ModelState.IsValid) { }
            return new JsonResult(new { });
        }

    }

    public class UploadModel
    {
    }
}
