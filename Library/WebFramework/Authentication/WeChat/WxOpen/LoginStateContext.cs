using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace WebFramework.Authentication.WeChat.WxOpen
{
    /// <summary>
    /// 用户进行登录操作时所需要的上下文信息
    /// </summary>
    public class WxOpenLoginStateContext : WxOpenServerResultContext
    {
        public WxOpenLoginStateContext(
            HttpContext context,
            AuthenticationScheme scheme,
            WxOpenLoginOptions options,
            string sessionKey,
            string openId,
            string uniodId,
            string errCode,
            string errMsg,
            string sessionInfoKey = null) : base(context, scheme, options, sessionKey, openId, uniodId, errCode, errMsg)
        {
            SessionInfoKey = sessionInfoKey;
        }

        /// <summary>
        /// 微信服务端返回的密匙保存在缓存中关联的Key
        /// </summary>
        public string SessionInfoKey { get; set; }
    }

    /// <summary>
    /// 微信服务器返回的信息以及当前验证处理的上下文信息【会话密钥】
    /// </summary>
    public class WxOpenServerResultContext : ResultContext<WxOpenLoginOptions>
    {
        public WxOpenServerResultContext(
            HttpContext context,
            AuthenticationScheme scheme,
            WxOpenLoginOptions options,
            string sessionKey,
            string openId,
            string uniodId,
            string errCode,
            string errMsg) : base(context, scheme, options)
        {
            SessionKey = sessionKey;
            OpenId = openId;
            UnionId = uniodId;
            ErrCode = errCode;
            ErrMsg = errMsg;
        }

        /// <summary>
        /// 会话密钥【请注意该信息的安全性,不要下发至客户端】
        /// </summary>
        public string SessionKey { get; set; }

        /// <summary>
        /// 用户唯一标识【请注意该信息的安全性,不要下发至客户端】
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 用户在开放平台的唯一标识符，在满足UnionId下发条件的情况下会返回【请注意该信息的安全性,不要下发至客户端】
        /// </summary>
        public string UnionId { get; set; }

        /// <summary>
        /// 错误码
        /// </summary>
        public string ErrCode { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrMsg { get; set; }
    }
}
