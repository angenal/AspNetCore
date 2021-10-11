using System;

namespace WebControllers.Models.DTO
{
    /// <summary>
    ///
    /// </summary>
    public class CaptchaCodeOutputDto
    {
        /// <summary>
        /// 验证码生成参数
        /// </summary>
        public string LastCode { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpireAt { get; set; }
    }
    /// <summary>
    ///
    /// </summary>
    public class CaptchaCodeComfirmInputDto
    {
        /// <summary>
        /// 验证码生成参数
        /// </summary>
        public string LastCode { get; set; }
        /// <summary>
        /// 验证码输入值
        /// </summary>
        public string CaptchaCode { get; set; }
    }
    /// <summary>
    ///
    /// </summary>
    public class CaptchaCodeComfirmOutputDto
    {
        /// <summary>
        /// 验证码生成参数
        /// </summary>
        public string LastCode { get; set; }
        /// <summary>
        /// 验证码输入值
        /// </summary>
        public string CaptchaCode { get; set; }
        /// <summary>
        /// 是否正确
        /// </summary>
        public bool Correct { get; set; }
        /// <summary>
        /// 是否已过期
        /// </summary>
        public bool Expired { get; set; }
    }

}
