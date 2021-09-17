using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using WebCore;
using WebCore.Security;
using WebFramework.Models.DTO;
using WebInterface;
using WebInterface.Settings;

namespace WebFramework.Controllers
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
        private readonly IPdfTools pdf;

        /// <summary></summary>
        public FileController(IWebHostEnvironment env, IConfiguration config, ICrypto crypto, IMemoryCache cache, IPdfTools pdf)
        {
            this.env = env;
            this.config = config;
            this.crypto = crypto;
            this.cache = cache;
            this.pdf = pdf;
        }


        #region 上传文件 api/File/Upload

        /// <summary>
        /// 上传文件
        /// </summary>
        [HttpPost]
        [DisableFormModelBinding]
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
        [Produces("application/json")]
        [ProducesResponseType(typeof(FileSignOutputDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorJsonResultObject), (int)HttpStatusCode.BadRequest)]
        public IActionResult PdfFileSign(PdfFileSignInputDto input)
        {
            if (!input.Path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) return Error("文件格式错误!");
            if (!input.Path.StartsWith("/")) return Error("文件不存在!");
            var file = Path.Combine(env.WebRootPath, input.Path.TrimStart('/'));
            if (!System.IO.File.Exists(file)) return Error("文件不存在!");

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
        [Produces("application/json")]
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
        [Produces("application/json")]
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
        /// 图片验证码
        /// </summary>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CaptchaCodeOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult CaptchaCode()
        {
            var result = new CaptchaCodeOutputDto
            {
                Value = 4.RandomString(),
                ExpireAt = DateTime.Now.AddMinutes(1),
            };
            byte[] json = Encodings.Utf8.GetBytes(result.ToJson()), key = Encodings.Utf8.GetBytes(AesSettings.Instance.Key), iv = Encodings.Utf8.GetBytes(AesSettings.Instance.IV);
            result.Value = Encodings.Utf8.GetString(crypto.AESEncrypt(json, key, iv));
            return Ok(result);
        }

        /// <summary>
        /// 验证码图片
        /// </summary>
        [HttpGet]
        [Produces("image/jpeg")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult CaptchaCode([FromQuery] string lastCode, int width = 90, int height = 36, int fontSize = 20, int length = 4, int lines = 4)
        {
            var captchaCode = (length > 1 ? length : 1).RandomString();
            if (!string.IsNullOrWhiteSpace(lastCode)) cache.Set(lastCode, captchaCode, TimeSpan.FromMinutes(1));

            var stream = new MemoryStream();
            using var image = new Bitmap(width, height, PixelFormat.Format64bppArgb);
            using var graphics = Graphics.FromImage(image);

            //清空图片背景色
            graphics.Clear(Color.FromArgb(240, 243, 248));

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
            var r = new Random();
            for (int i = 0; i < 80; i++)
            {
                int x = r.Next(width), y = r.Next(height);
                image.SetPixel(x, y, Color.FromArgb(r.Next()));
            }
            for (var i = 0; i < 25; i++)
            {
                int x1 = r.Next(width), x2 = r.Next(width), y1 = r.Next(height), y2 = r.Next(height);
                graphics.DrawLine(new Pen(Colors[r.Next(0, 5)], 1), new PointF(x1, y1), new PointF(x2, y2));
            }
            for (var i = 0; i < 80; i++)
            {
                int x = r.Next(width), y = r.Next(height);
                graphics.DrawLine(new Pen(Colors[r.Next(0, 5)], 1), new PointF(x, y), new PointF(x + 1, y + 1));
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
        static readonly Color[] Colors = { Color.FromArgb(37, 72, 91), Color.FromArgb(68, 24, 25), Color.FromArgb(17, 46, 2), Color.FromArgb(70, 16, 100), Color.FromArgb(24, 88, 74) };
        /// <summary>
        /// 随机文字字体
        /// </summary>
        static Font[] TextFonts(int fontSize) => new Font[]{
           new Font(new FontFamily("Arial"), fontSize, FontStyle.Bold | FontStyle.Italic),
           new Font(new FontFamily("Georgia"), fontSize, FontStyle.Bold | FontStyle.Italic),
           new Font(new FontFamily("Times New Roman"), fontSize, FontStyle.Bold | FontStyle.Italic),
           new Font(new FontFamily("Comic Sans MS"), fontSize, FontStyle.Bold | FontStyle.Italic)
        };

        #endregion

    }
}
