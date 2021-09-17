using System;

namespace WebFramework.Models.DTO
{
    /// <summary>
    ///
    /// </summary>
    public class CaptchaCodeOutputDto
    {
        /// <summary>
        /// 验证码
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpireAt { get; set; }
    }
}
