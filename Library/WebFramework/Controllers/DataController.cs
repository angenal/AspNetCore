using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
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
        [Produces("application/json")]
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
        [Produces("application/json")]
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
        [Produces("application/json")]
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
        [Produces("application/json")]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult Encode([FromBody] EncodeTextInputDto input)
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
        [Produces("application/json")]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult Decode([FromBody] EncodeTextInputDto input)
        {
            var result = new EncodeTextOutputDto()
            {
                Text = crypto.FromBase64String(input.Text)
            };
            return Ok(result);
        }

        /// <summary>
        /// 文本加密 = CryptoJS.AES.encrypt  /data/crypto.html
        /// </summary>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult Encrypt([FromBody] EncryptTextInputDto input)
        {
            if (input.Keys == null) input.Keys = new EncryptKeyPairInputDto
            {
                Key = AesSettings.Instance.Key,
                Iv = AesSettings.Instance.IV,
            };

            var key = Encoding.UTF8.GetBytes(input.Keys.Key);
            var iv = Encoding.UTF8.GetBytes(input.Keys.Iv);

            var encryptToBytes = EncryptToBytes(input.Text, key, iv);
            string base64String = Convert.ToBase64String(encryptToBytes, 0, encryptToBytes.Length);

            var result = new EncodeTextOutputDto() { Text = base64String };
            return Ok(result);
        }

        /// <summary>
        /// 文本解密 = CryptoJS.AES.decrypt  /data/crypto.html
        /// </summary>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(EncodeTextOutputDto), (int)HttpStatusCode.OK)]
        public IActionResult Decrypt([FromBody] DecryptTextInputDto input)
        {
            if (input.Keys == null) input.Keys = new EncryptKeyPairInputDto
            {
                Key = AesSettings.Instance.Key,
                Iv = AesSettings.Instance.IV,
            };

            var key = Encoding.UTF8.GetBytes(input.Keys.Key);
            var iv = Encoding.UTF8.GetBytes(input.Keys.Iv);

            var encrypted = Convert.FromBase64String(input.Text);
            var decriptedFromJavascript = DecryptFromBytes(encrypted, key, iv);

            var result = new EncodeTextOutputDto() { Text = decriptedFromJavascript };
            return Ok(result);
        }

        /// <summary>
        /// 文本加密
        /// </summary>
        private static string DecryptFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            // Declare the string used to hold the decrypted text.
            string plaintext = null;
            // Create an RijndaelManaged object with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                //Settings
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;
                rijAlg.Key = key;
                rijAlg.IV = iv;
                // Create a decrytor to perform the stream transform.
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }

        /// <summary>
        /// 文本解密
        /// </summary>
        private static byte[] EncryptToBytes(string plainText, byte[] key, byte[] iv)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            byte[] encrypted;
            // Create a RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;
                rijAlg.Key = key;
                rijAlg.IV = iv;
                // Create a decrytor to perform the stream transform.
                var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

    }
}
