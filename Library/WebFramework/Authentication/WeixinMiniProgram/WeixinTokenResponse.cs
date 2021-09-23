using System;
using System.Text.Json;

namespace WebFramework.Authentication.WeixinMiniProgram
{
    /// <summary>
    /// 微信小程序登录凭证校验后所返回的结果。
    /// https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/login/auth.code2Session.html
    /// </summary>
    public class WeixinTokenResponse : IDisposable
    {
        /// <summary>
        /// 用户唯一标识
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 会话密钥
        /// </summary>
        public string SessionKey { get; set; }

        /// <summary>
        /// 用户在开放平台的唯一标识符，在满足 UnionID 下发条件的情况下会返回
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

        private WeixinTokenResponse(JsonDocument response)
        {
            Response = response;
            var root = response.RootElement;
            OpenId = root.GetString("openid");
            SessionKey = root.GetString("session_key");
            UnionId = root.GetString("unionid");
            ErrCode = root.GetString("errcode");
            ErrMsg = root.GetString("errmsg");
        }

        private WeixinTokenResponse(Exception error)
        {
            Error = error;
        }

        /// <summary></summary>
        public static WeixinTokenResponse Success(JsonDocument response)
        {
            return new WeixinTokenResponse(response);
        }

        /// <summary></summary>
        public static WeixinTokenResponse Failed(Exception error)
        {
            return new WeixinTokenResponse(error);
        }

        /// <summary></summary>
        public void Dispose()
        {
            Response?.Dispose();
        }
    }

    /// <summary></summary>
    public static class JsonDocumentAuthExtensions
    {
        /// <summary></summary>
        public static string GetString(this JsonElement element, string key)
        {
            return element.TryGetProperty(key, out var property) && property.ValueKind != JsonValueKind.Null ? property.ToString() : null;
        }
    }
}
