using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Text;
using WebCore;
using WebCore.Platform;
using WebFramework.Models.DTO;
using WebInterface;
using WebInterface.Settings;

namespace WebFramework.Controllers
{
    /// <summary>
    /// 数据
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DataController : ApiController
    {
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration config;
        private readonly ICrypto crypto;
        private readonly IMemoryCache cache;

        /// <summary></summary>
        public DataController(IWebHostEnvironment env, IConfiguration config, ICrypto crypto, IMemoryCache cache)
        {
            this.env = env;
            this.config = config;
            this.crypto = crypto;
            this.cache = cache;
        }

        /// <summary>
        /// 系统状态
        /// </summary>
        [HttpGet]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult AppStatus()
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

        /// <summary>
        /// 设备ID
        /// </summary>
        [HttpGet]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult DeviceId()
        {
            var id = OS.GetDeviceId();
            return Ok(new { id });
        }

        /// <summary>
        /// 线程ID
        /// </summary>
        [HttpGet]
        [Produces(Produces.JSON)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult ThreadId()
        {
            var id = OS.GetCurrentThreadId();
            return Ok(new { id, pid = Environment.ProcessId });
        }

        /// <summary>
        /// 文本Base64编码 = btoa(encodeURIComponent(text))
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult Base64Encode([FromBody] EncodeTextInputDto input)
        {
            var result = new EncodeTextOutputDto()
            {
                Text = crypto.ToBase64String(input.Text)
            };
            return Ok(result);
        }

        /// <summary>
        /// 文本Base64解码 = decodeURIComponent(atob(text))
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult Base64Decode([FromBody] EncodeTextInputDto input)
        {
            var result = new EncodeTextOutputDto()
            {
                Text = crypto.FromBase64String(input.Text)
            };
            return Ok(result);
        }

        /// <summary>
        /// 文本加密 (authenticated encryption)
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult Encrypt([FromBody] EncryptText1InputDto input)
        {
            var key = Encoding.UTF8.GetBytes(input.Keys.Key);
            var nonce = Encoding.UTF8.GetBytes(input.Keys.Nonce);

            var encryptToBytes = crypto.Encrypt(Encoding.UTF8.GetBytes(input.Text), nonce, key);
            string base64String = Convert.ToBase64String(encryptToBytes, 0, encryptToBytes.Length);

            var result = new EncodeTextOutputDto() { Text = base64String };
            return Ok(result);
        }

        /// <summary>
        /// 文本解密 (authenticated encryption)
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult Decrypt([FromBody] DecryptText1InputDto input)
        {
            var key = Encoding.UTF8.GetBytes(input.Keys.Key);
            var nonce = Encoding.UTF8.GetBytes(input.Keys.Nonce);

            var encrypted = Convert.FromBase64String(input.Text);
            var decriptedBytes = crypto.Decrypt(encrypted, nonce, key);

            var text = Encoding.UTF8.GetString(decriptedBytes);
            var result = new EncodeTextOutputDto() { Text = text };
            return Ok(result);
        }

        /// <summary>
        /// 文本加密 (ChaCha20)
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult EncryptChaCha20([FromBody] EncryptText2InputDto input)
        {
            var key = Encoding.UTF8.GetBytes(input.Keys.Key);
            var nonce = Encoding.UTF8.GetBytes(input.Keys.Nonce);

            var encryptToBytes = crypto.EncryptChaCha20(Encoding.UTF8.GetBytes(input.Text), nonce, key);
            string base64String = Convert.ToBase64String(encryptToBytes, 0, encryptToBytes.Length);

            var result = new EncodeTextOutputDto() { Text = base64String };
            return Ok(result);
        }

        /// <summary>
        /// 文本解密 (ChaCha20)
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult DecryptChaCha20([FromBody] DecryptText2InputDto input)
        {
            var key = Encoding.UTF8.GetBytes(input.Keys.Key);
            var nonce = Encoding.UTF8.GetBytes(input.Keys.Nonce);

            var encrypted = Convert.FromBase64String(input.Text);
            var decriptedBytes = crypto.DecryptChaCha20(encrypted, nonce, key);

            var text = Encoding.UTF8.GetString(decriptedBytes);
            var result = new EncodeTextOutputDto() { Text = text };
            return Ok(result);
        }

        /// <summary>
        /// 文本加密 = CryptoJS.AES.encrypt  /data/crypto.html
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult AESCBCPkcs7Encrypt([FromBody] EncryptTextInputDto input)
        {
            if (input.Keys == null) input.Keys = new EncryptKeyPairInputDto
            {
                Key = AesSettings.Instance.Key,
                Iv = AesSettings.Instance.IV,
            };

            var key = Encoding.UTF8.GetBytes(input.Keys.Key);
            var iv = Encoding.UTF8.GetBytes(input.Keys.Iv);

            var encryptToBytes = crypto.AESCBCPkcs7Encrypt(input.Text, key, iv);
            string base64String = Convert.ToBase64String(encryptToBytes, 0, encryptToBytes.Length);

            var result = new EncodeTextOutputDto() { Text = base64String };
            return Ok(result);
        }

        /// <summary>
        /// 文本解密 = CryptoJS.AES.decrypt  /data/crypto.html
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult AESCBCPkcs7Decrypt([FromBody] DecryptTextInputDto input)
        {
            if (input.Keys == null) input.Keys = new EncryptKeyPairInputDto
            {
                Key = AesSettings.Instance.Key,
                Iv = AesSettings.Instance.IV,
            };

            var key = Encoding.UTF8.GetBytes(input.Keys.Key);
            var iv = Encoding.UTF8.GetBytes(input.Keys.Iv);

            var encrypted = Convert.FromBase64String(input.Text);
            var decriptedFromJavascript = crypto.AESCBCPkcs7Decrypt(encrypted, key, iv);

            var result = new EncodeTextOutputDto() { Text = decriptedFromJavascript };
            return Ok(result);
        }

        /// <summary>
        /// 文本加密 (AES 256 + GCM)
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult AES256GCMEncrypt([FromBody] EncryptText3InputDto input)
        {
            var key = Encoding.UTF8.GetBytes(input.Keys.Key);
            var nonce = Encoding.UTF8.GetBytes(input.Keys.Nonce);

            var encryptToBytes = crypto.AES256GCMEncrypt(Encoding.UTF8.GetBytes(input.Text), nonce, key);
            string base64String = Convert.ToBase64String(encryptToBytes, 0, encryptToBytes.Length);

            var result = new EncodeTextOutputDto() { Text = base64String };
            return Ok(result);
        }

        /// <summary>
        /// 文本解密 (AES 256 + GCM)
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult AES256GCMDecrypt([FromBody] DecryptText3InputDto input)
        {
            var key = Encoding.UTF8.GetBytes(input.Keys.Key);
            var nonce = Encoding.UTF8.GetBytes(input.Keys.Nonce);

            var encrypted = Convert.FromBase64String(input.Text);
            var decriptedBytes = crypto.AES256GCMDecrypt(encrypted, nonce, key);

            var text = Encoding.UTF8.GetString(decriptedBytes);
            var result = new EncodeTextOutputDto() { Text = text };
            return Ok(result);
        }

        /// <summary>
        /// 文本加密 (RSA动态加密)
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult RSAEncrypt([FromBody] EncodeTextInputDto input)
        {
            var encryptToBytes = crypto.RSAEncrypt(Encoding.UTF8.GetBytes(input.Text));
            string base64String = Convert.ToBase64String(encryptToBytes, 0, encryptToBytes.Length);

            var result = new EncodeTextOutputDto() { Text = base64String };
            return Ok(result);
        }

        /// <summary>
        /// 文本解密 (RSA动态解密)
        /// </summary>
        [HttpPost]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult RSADecrypt([FromBody] EncodeTextInputDto input)
        {
            var encrypted = Convert.FromBase64String(input.Text);
            var decriptedBytes = crypto.RSADecrypt(encrypted);

            var text = Encoding.UTF8.GetString(decriptedBytes);
            var result = new EncodeTextOutputDto() { Text = text };
            return Ok(result);
        }
    }
}
