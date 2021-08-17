using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using WebCore;
using WebCore.Security;

namespace WebFramework.Controllers
{
    /// <summary>
    /// 文件
    /// </summary>
    [ApiController]
    [DisableFormModelBinding]
    [Route("api/[controller]/[action]")]
    public class FileController : ApiController
    {
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration config;
        private readonly IMemoryCache cache;

        /// <summary>
        ///
        /// </summary>
        public FileController(IWebHostEnvironment env, IConfiguration config, IMemoryCache cache)
        {
            this.env = env;
            this.config = config;
            this.cache = cache;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "Upload")]
        [Produces("application/json")]
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

            // upload file
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
        /// <summary>
        /// Upload File Output
        /// </summary>
        public class UploadFileOutputDto
        {
            /// <summary>
            /// the form field name.
            /// </summary>
            public string Key { get; set; }
            /// <summary>
            /// the file name.
            /// </summary>
            public string Value { get; set; }
            /// <summary>
            /// the raw Content-Type header of the uploaded file.
            /// </summary>
            public string ContentType { get; set; }
            /// <summary>
            /// the file length in bytes.
            /// </summary>
            public long Length { get; set; }
            /// <summary>
            /// Output File Path.
            /// </summary>
            public string Path { get; set; }
        }


        /// <summary>
        /// 生成签名密钥 by Minisign
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<UploadFileOutputDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorJsonResultObject), (int)HttpStatusCode.BadRequest)]
        public IActionResult MinisignGenerateKey(MinisignGenerateKeyInputDto input)
        {
            var path = Path.Combine(env.WebRootPath, "minisign");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (string.IsNullOrWhiteSpace(input.FileName)) input.FileName = "minisign";
            string privateKeyFile = Path.Combine(path, $"{input.FileName}.key"), publicKeyFile = Path.Combine(path, $"{input.FileName}.pub");

            var existsKey = System.IO.File.Exists(privateKeyFile) && System.IO.File.Exists(publicKeyFile);
            if (existsKey && input.Renew.HasValue && input.Renew == true)
            {
                System.IO.File.Delete(privateKeyFile);
                System.IO.File.Delete(publicKeyFile);
            }


            var keyPair = Minisign.GenerateKeyPair(input.KeyPass, true, path, input.FileName);
            privateKeyFile = keyPair.MinisignPrivateKeyFilePath; publicKeyFile = keyPair.MinisignPublicKeyFilePath;
            if (!System.IO.File.Exists(privateKeyFile) || !System.IO.File.Exists(publicKeyFile))
                return Error("生成失败!");

            var privateKey = Minisign.LoadPrivateKeyFromFile(privateKeyFile, input.KeyPass);
            var publicKey = Minisign.LoadPublicKeyFromFile(publicKeyFile);
            if (!privateKey.KeyId.Equals(publicKey.KeyId))
                return Error("生成失败!");

            return Ok(new { keyId = Sodium.Utilities.BinaryToHex(publicKey.KeyId), keyPass = input.KeyPass });
        }
        /// <summary>
        /// 生成签名密钥 Minisign
        /// </summary>
        public class MinisignGenerateKeyInputDto
        {
            /// <summary>
            /// 安全密钥
            /// </summary>
            [Required]
            [StringLength(64, MinimumLength = 8, ErrorMessage = "安全密钥字符长度为8～64")]
            public string KeyPass { get; set; }
            /// <summary>
            /// 证书文件;不包括文件扩展名,默认minisign
            /// </summary>
            public string FileName { get; set; } = "minisign";
            /// <summary>
            /// 重新生成
            /// </summary>
            public bool? Renew { get; set; }
        }

        /// <summary>
        /// 文件签名 by Minisign
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<UploadFileOutputDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorJsonResultObject), (int)HttpStatusCode.BadRequest)]
        public IActionResult MinisignFileSign(MinisignFileSignInputDto input)
        {
            var path = Path.Combine(env.WebRootPath, "minisign");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (string.IsNullOrWhiteSpace(input.FileName)) input.FileName = "minisign";
            string privateKeyFile = Path.Combine(path, $"{input.FileName}.key"), publicKeyFile = Path.Combine(path, $"{input.FileName}.pub");

            var existsKey = System.IO.File.Exists(privateKeyFile) && System.IO.File.Exists(publicKeyFile);
            if (!existsKey) return Error("签名文件不存在!");

            var privateKey = Minisign.LoadPrivateKeyFromFile(privateKeyFile, input.KeyPass);
            var publicKey = Minisign.LoadPublicKeyFromFile(publicKeyFile);
            if (!privateKey.KeyId.Equals(publicKey.KeyId))
                return Error("签名文件错误!");

            var file = Path.Combine(env.WebRootPath, input.Path);
            var signedFile = Minisign.Sign(file, privateKey);

            var signature = Minisign.LoadSignatureFromFile(signedFile);
            if (!input.KeyId.Equals(Sodium.Utilities.BinaryToHex(signature.KeyId)) || !input.KeyId.Equals(Sodium.Utilities.BinaryToHex(publicKey.KeyId)))
                return Error("安全密钥错误!");

            if (!Minisign.ValidateSignature(file, signature, publicKey))
                return Error("签名失败!");

            return Ok(new { signedFile });
        }
        /// <summary>
        /// 文件签名 by Minisign
        /// </summary>
        public class MinisignFileSignInputDto
        {
            /// <summary>
            /// 被签名文件的路径
            /// </summary>
            [Required]
            public string Path { get; set; }
            /// <summary>
            /// 安全密钥Id
            /// </summary>
            [Required]
            public string KeyId { get; set; }
            /// <summary>
            /// 安全密钥
            /// </summary>
            [Required]
            [StringLength(64, MinimumLength = 8, ErrorMessage = "安全密钥字符长度为8～64")]
            public string KeyPass { get; set; }
            /// <summary>
            /// 证书文件;不包括文件扩展名,默认minisign
            /// </summary>
            [Required]
            public string FileName { get; set; } = "minisign";
        }


        /// <summary>
        /// 图片验证码
        /// </summary>
        [HttpGet]
        [Produces("image/jpeg")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult CaptchaCode([FromQuery] string lastCode, int imageWidth = 90, int imageHeight = 36, int fontSize = 20, int length = 4, int lines = 4)
        {
            var captchaCode = (length > 1 ? length : 1).RandomString();
            if (!string.IsNullOrWhiteSpace(lastCode)) cache.Set(lastCode, captchaCode, TimeSpan.FromMinutes(1));

            var stream = new MemoryStream();
            using var image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format64bppArgb);
            using var graphics = Graphics.FromImage(image);

            //清空图片背景色
            graphics.Clear(Color.White);

            //画图片的背景噪音线
            var random = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i <= lines; i++)
            {
                int x1 = random.Next(image.Width), x2 = random.Next(image.Width), y1 = random.Next(image.Height), y2 = random.Next(image.Height);
                graphics.DrawLine(new Pen(Color.FromArgb(random.Next(255), random.Next(255), random.Next(255))), x1, y1, x2, y2);
            }

            //随机文字颜色
            int c = random.Next(100) % TextColors.Length;
            Color imageTextColor1 = TextColors[c].Item1, imageTextColor2 = TextColors[c].Item2;
            var f = TextFonts(fontSize);
            c = random.Next(100) % f.Length;
            var brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), imageTextColor1, imageTextColor2, 1.2f, true);
            graphics.DrawString(captchaCode, f[c], brush, 2, 2);

            //画图片的前景噪音点
            for (int i = 0; i < 80; i++)
            {
                int x = random.Next(image.Width), y = random.Next(image.Height);
                image.SetPixel(x, y, Color.FromArgb(random.Next()));
            }

            //画图片的边框线
            graphics.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);

            image.Save(stream, ImageFormat.Jpeg);
            stream.Position = 0;

            return File(stream, "image/jpeg");
        }
        /// <summary>
        /// 随机文字颜色
        /// </summary>
        static readonly Tuple<Color, Color>[] TextColors =
        {
            new Tuple<Color, Color>(Color.FromArgb(65, 133, 235), Color.FromArgb(142, 24, 232)),
            new Tuple<Color, Color>(Color.FromArgb(52, 116, 235), Color.FromArgb(251, 40, 40)),
            new Tuple<Color, Color>(Color.FromArgb(200, 68, 235), Color.FromArgb(61, 53, 235)),
            new Tuple<Color, Color>(Color.FromArgb(255, 95, 89), Color.FromArgb(95, 13, 255)),
        };
        /// <summary>
        /// 随机文字字体
        /// </summary>
        static Font[] TextFonts(int fontSize) => new Font[]{
           new Font(new FontFamily("Arial"), fontSize, FontStyle.Bold | FontStyle.Italic),
           new Font(new FontFamily("Georgia"), fontSize, FontStyle.Bold | FontStyle.Italic),
           new Font(new FontFamily("Times New Roman"), fontSize, FontStyle.Bold | FontStyle.Italic),
           new Font(new FontFamily("Comic Sans MS"), fontSize, FontStyle.Bold | FontStyle.Italic)
        };
    }
}
