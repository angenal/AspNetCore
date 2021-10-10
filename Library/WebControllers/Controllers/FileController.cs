using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using WebCore;
using WebCore.Security;
using WebFramework;
using WebFramework.Models.DTO;
using WebInterface;

namespace WebControllers.Controllers
{
    /// <summary>
    /// 文件
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FileController : ApiController
    {
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration config;
        private readonly ICrypto crypto;
        private readonly IMemoryCache cache;
        private readonly IImageTools image;
        private readonly IPdfTools pdf;

        /// <summary></summary>
        public FileController(IWebHostEnvironment env, IConfiguration config, ICrypto crypto, IMemoryCache cache, IImageTools image, IPdfTools pdf)
        {
            this.env = env;
            this.config = config;
            this.crypto = crypto;
            this.cache = cache;
            this.image = image;
            this.pdf = pdf;
        }


        #region 上传文件 api/File/Upload

        /// <summary>
        /// 上传文件
        /// </summary>
        [HttpPost]
        [DisableFormModelBinding]
        //[DisableRequestSizeLimit]
        //[RequestSizeLimit(524288000)] // 500MB
        [Authorize(Policy = "Upload")]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(List<UploadFileOutputDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorJsonResultObject), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Upload()
        {
            var section = config.GetSection("Upload:WebRootPath");
            if (!section.Exists()) return Error("未配置该功能");
            if (!Request.HasFormContentType) return Error("未上传任何文件");

            var root = section.Value.Trim('/');
            var path = Path.Combine(env.WebRootPath, root);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            var url = Request.RootPath() + root;
            var files = new List<UploadFileOutputDto>();

            //var buffer = new byte[4096];
            var result = await Request.UploadFile(async file =>
            {
                var name = (Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName)).ToLower();
                using (var stream = System.IO.File.Create(Path.Combine(path, name))) await file.CopyToAsync(stream);
                files.Add(new UploadFileOutputDto { Key = file.Name, Value = file.FileName, ContentType = file.ContentType, Length = file.Length, Path = $"{url}/{name}" });
                //using (var stream = file.OpenReadStream()) while (await stream.ReadAsync(buffer.AsMemory(0, buffer.Length)) > 0) { }
            });

            // ModelState is still validated from model
            //if (!ModelState.IsValid) { }

            return new JsonResult(files);
        }

        #endregion


        #region PDF数字签名 api/File/PdfFileSign

        /// <summary>
        /// PDF数字签名 by SignLib
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(FileSignOutputDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorJsonResultObject), (int)HttpStatusCode.BadRequest)]
        public IActionResult PdfFileSign(PdfFileSignInputDto input)
        {
            if (!input.Path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) return Error("文件格式错误!");
            if (!input.Path.StartsWith("/")) return Error("文件不存在!");
            var file = Path.Combine(env.WebRootPath, input.Path.TrimStart('/'));
            //if (!System.IO.File.Exists(file)) return Error("文件不存在!");

            var signedFile = file.Substring(0, file.Length - 4) + "Signed.pdf";
            // Sign by the signature certificate file: PdfCert.pfx
            pdf.Sign(file, signedFile);

            var result = new FileSignOutputDto { Path = signedFile };
            return Ok(result);
        }

        #endregion


        #region 文件签名 api/File/MinisignFileSign

        /// <summary>
        /// 配置密钥 for Minisign
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(MinisignKeyOutputDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorJsonResultObject), (int)HttpStatusCode.BadRequest)]
        public IActionResult MinisignGenerateKey(MinisignGenerateKeyInputDto input)
        {
            var path = Path.Combine(env.WebRootPath, "file", "minisign");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (string.IsNullOrWhiteSpace(input.KeyFile)) input.KeyFile = "minisign";
            string privateKeyFile = Path.Combine(path, $"{input.KeyFile}.key"), publicKeyFile = Path.Combine(path, $"{input.KeyFile}.pub");

            var existsKey = System.IO.File.Exists(privateKeyFile) && System.IO.File.Exists(publicKeyFile);
            if (existsKey && input.Renew.HasValue && input.Renew == true)
            {
                System.IO.File.Delete(privateKeyFile); System.IO.File.Delete(publicKeyFile);
            }

            var keyPair = Minisign.GenerateKeyPair(input.KeyPass, true, path, input.KeyFile);
            privateKeyFile = keyPair.MinisignPrivateKeyFilePath; publicKeyFile = keyPair.MinisignPublicKeyFilePath;
            if (!System.IO.File.Exists(privateKeyFile) || !System.IO.File.Exists(publicKeyFile))
                return Error("生成失败!");

            var privateKey = Minisign.LoadPrivateKeyFromFile(privateKeyFile, input.KeyPass);
            var publicKey = Minisign.LoadPublicKeyFromFile(publicKeyFile);
            string privateKeyId = privateKey.KeyId.BinaryToHex(), publicKeyId = publicKey.KeyId.BinaryToHex();
            if (!privateKeyId.Equals(publicKeyId))
                return Error("生成失败!");

            var result = new MinisignKeyOutputDto
            {
                KeyId = publicKeyId,
                KeyPass = input.KeyPass,
                KeyFile = input.KeyFile
            };
            return Ok(result);
        }

        /// <summary>
        /// 文件签名 by Minisign
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(FileSignOutputDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorJsonResultObject), (int)HttpStatusCode.BadRequest)]
        public IActionResult MinisignFileSign(MinisignFileSignInputDto input)
        {
            if (!input.Path.StartsWith("/")) return Error("文件不存在!");
            var file = Path.Combine(env.WebRootPath, input.Path.TrimStart('/'));
            if (!System.IO.File.Exists(file)) return Error("文件不存在!");

            var path = Path.Combine(env.WebRootPath, "file", "minisign");
            if (!Directory.Exists(path)) return Error("签名密钥未找到!");
            if (string.IsNullOrWhiteSpace(input.KeyFile)) input.KeyFile = "minisign";
            string privateKeyFile = Path.Combine(path, $"{input.KeyFile}.key"), publicKeyFile = Path.Combine(path, $"{input.KeyFile}.pub");

            var existsKey = System.IO.File.Exists(privateKeyFile) && System.IO.File.Exists(publicKeyFile);
            if (!existsKey) return Error("安全密钥文件不存在!");

            var privateKey = Minisign.LoadPrivateKeyFromFile(privateKeyFile, input.KeyPass);
            var publicKey = Minisign.LoadPublicKeyFromFile(publicKeyFile);
            if (!privateKey.KeyId.Compare(publicKey.KeyId))
                return Error("安全密钥错误!");
            if (!input.KeyId.Equals(publicKey.KeyId.BinaryToHex()))
                return Error("安全密钥Id错误!");

            var signedFile = Minisign.Sign(file, privateKey);
            var signature = Minisign.LoadSignatureFromFile(signedFile);
            if (!input.KeyId.Equals(signature.KeyId.BinaryToHex()))
                return Error("文件签名失败!");
            if (!Minisign.ValidateSignature(file, signature, publicKey))
                return Error("文件签名失败!");

            var result = new FileSignOutputDto { Path = input.Path + ".minisig" };
            return Ok(result);
        }

        #endregion


        #region 图片验证码 api/File/CaptchaCode

        /// <summary>
        /// 创建验证码
        /// </summary>
        /// <param name="expireSeconds">过期时间(单位/秒):默认60秒,最多10分钟</param>
        [HttpPost("{expireSeconds=60}")]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(CaptchaCodeOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult CaptchaCode(string expireSeconds)
        {
            if (!int.TryParse(expireSeconds, out int value)) value = 60;
            if (value < 10) return Error("参数错误:过期时间不能少于10秒!");
            if (value > 600) value = 600;
            var time = DateTime.Now.AddSeconds(value);
            var lastCode = image.NewCaptchaCode(time);
            if (lastCode == 0) return CaptchaCode(expireSeconds);
            var result = new CaptchaCodeOutputDto { ExpireAt = time, LastCode = lastCode.ToString() };
            return Ok(result);
        }

        /// <summary>
        /// 生成验证码图片
        /// </summary>
        /// <param name="lastCode">验证码参数</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="degree">难度系数 1.低 2.高</param>
        [HttpGet]
        [Produces("image/jpeg")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult CaptchaCode([FromQuery] string lastCode, int width = 90, int height = 36, int fontSize = 20, int degree = 1)
        {
            if (!ulong.TryParse(lastCode, out ulong key)) return NotFound();
            var captchaCode = image.GetCaptchaCode(key);
            if (captchaCode == null) return NotFound();
            var stream = image.NewCaptchaCode(captchaCode, width, height, fontSize, degree);
            return File(stream, "image/jpeg");
        }

        /// <summary>
        /// 确认验证码输入
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(CaptchaCodeComfirmOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult CaptchaComfirm([FromBody] CaptchaCodeComfirmInputDto input)
        {
            var result = new CaptchaCodeComfirmOutputDto { LastCode = input.LastCode, CaptchaCode = input.CaptchaCode };
            if (!ulong.TryParse(input.LastCode, out ulong key)) return Ok(result);
            var captchaCode = image.GetCaptchaCode(key);
            result.Expired = captchaCode == null;
            if (!result.Expired) result.Correct = captchaCode.Equals(input.CaptchaCode, StringComparison.OrdinalIgnoreCase);
            return Ok(result);
        }

        #endregion

    }
}
