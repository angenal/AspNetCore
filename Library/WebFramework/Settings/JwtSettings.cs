using System;
using System.ComponentModel;

namespace WebFramework.Settings
{
    /// <summary>
    /// JWT settings.
    /// </summary>
    public class JwtSettings
    {
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
