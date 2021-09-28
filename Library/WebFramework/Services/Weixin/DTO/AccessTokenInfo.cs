using System;

namespace WebFramework.Services.Weixin.DTO
{
    /// <summary>
    /// 统一的AccessToken信息
    /// </summary>
    [Serializable]
    public class AccessTokenInfo
    {
        /// <summary>
        /// 凭证信息
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 凭证过期时间
        /// </summary>
        public DateTime Expire { get; set; }
    }
}
