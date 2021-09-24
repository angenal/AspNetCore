using System;
using System.Text.Json;

namespace Microsoft.AspNetCore.Authentication.WxOpen
{
    /// <summary>
    /// 微信小程序登录凭证校验后返回的结果
    /// https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/login/auth.code2Session.html
    /// </summary>
    public class WxOpenPostResponse : IDisposable
    {
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

        /// <summary></summary>
        public Exception Error { get; set; }

        /// <summary></summary>
        public JsonDocument Response { get; set; }

        private WxOpenPostResponse(JsonDocument response)
        {
            var root = response.RootElement;
            SessionKey = GetString(root, "session_key");
            OpenId = GetString(root, "openid");
            UnionId = GetString(root, "unionid");
            ErrCode = GetString(root, "errcode");
            ErrMsg = GetString(root, "errmsg");
            Response = response;
        }

        private WxOpenPostResponse(Exception error)
        {
            Error = error;
        }

        /// <summary></summary>
        public static WxOpenPostResponse Success(JsonDocument response)
        {
            return new WxOpenPostResponse(response);
        }

        /// <summary></summary>
        public static WxOpenPostResponse Failed(Exception error)
        {
            return new WxOpenPostResponse(error);
        }

        /// <summary></summary>
        static string GetString(JsonElement element, string key)
        {
            return element.TryGetProperty(key, out var property) && property.ValueKind != JsonValueKind.Null ? property.ToString() : null;
        }

        /// <summary></summary>
        public void Dispose()
        {
            Response?.Dispose();
        }
    }
}
