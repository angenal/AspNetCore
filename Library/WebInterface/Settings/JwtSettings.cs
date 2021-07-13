using System;
using System.ComponentModel;

namespace WebInterface.Settings
{
    /// <summary>
    /// JWT settings.
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Default Instance.
        /// </summary>
        public static JwtSettings Instance = new JwtSettings();
        /// <summary>
        /// Configuration Section in appsettings.json
        /// </summary>
        public const string AppSettings = "JWT";
        /*
          "JWT": {
            "Issuer": "localhost",
            "Audience": "localhost",
            "SecretKey": "g9MiOLxxpcwUJJkbkzzr766137NkLNdUsawpF4uIKyo=",
            "EncryptionKey": "g9MiOLxxpcwUJJkb",
            "ExpireMinutes": 720,
            "ClockSkew": "00:05:00"
          }
        */

        /// <summary>
        /// EnvironmentVariable: SecretKey
        /// </summary>
        public const string EnvSecretKey = "JWT_SecretKey";
        /// <summary>
        /// EnvironmentVariable: EncryptionKey
        /// </summary>
        public const string EnvEncryptionKey = "JWT_EncryptionKey";
        /// <summary>
        /// HttpRequest Query Variable
        /// </summary>
        public const string HttpRequestQuery = "token";

        /// <summary>
        /// 谁颁发?
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// 哪些客户端用?
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// 签名密钥?
        /// </summary>
        public string SecretKey { get; set; }
        /// <summary>
        /// 加密密钥?
        /// </summary>
        public string EncryptionKey { get; set; }
        /// <summary>
        /// 过期(分钟数)?
        /// </summary>
        public int ExpireMinutes { get; set; }
        /// <summary>
        /// 时钟偏移(默认5分钟"00:05:00")
        /// </summary>
        [DefaultValue(300)]
        public TimeSpan ClockSkew { get; set; } = new TimeSpan(0, 5, 0);
    }
}
