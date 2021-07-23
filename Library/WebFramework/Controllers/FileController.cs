using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UploadStream;
using WebCore;

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
        private readonly IConfiguration config;

        /// <summary>
        ///
        /// </summary>
        public FileController(IWebHostEnvironment env, IConfiguration config)
        {
            this.env = env;
            this.config = config;
        }

        /// <summary>
        /// 上传
        /// </summary>
        [HttpPost("upload")]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Upload()
        {
            var section = config.GetSection("Upload:WebRootPath");
            if (!section.Exists()) return Error("未配置该功能");

            var root = section.Value.Trim('/');
            var path = Path.Combine(env.WebRootPath, root);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            var url = Request.RootPath() + root;
            var files = new List<object>();

            //var buffer = new byte[4096];
            //var files = new List<IFormFile>();
            await this.StreamFiles(async file =>
            {
                if (file.Length > 1)
                {
                    var filename = (Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName)).ToLower();
                    var filepath = Path.Combine(path, filename);
                    using (var stream = System.IO.File.Create(filepath)) await file.CopyToAsync(stream);
                    files.Add(new { file.Name, file.FileName, file.ContentDisposition, file.ContentType, file.Length, Path = $"{url}/{filename}" });
                }
                //using (var stream = file.OpenReadStream())
                //{
                //    while (await stream.ReadAsync(buffer.AsMemory(0, buffer.Length)) > 0) { }
                //}
                //files.Add(file);
            });

            // ModelState is still validated from model
            //if (!ModelState.IsValid) { }

            return new JsonResult(files);
        }

    }
}
