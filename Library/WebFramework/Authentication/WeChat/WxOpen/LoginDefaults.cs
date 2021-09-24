using Microsoft.AspNetCore.Authentication;

namespace WebFramework.Authentication.WeChat.WxOpen
{
    /// <summary>
    /// 微信小程序登录校验的默认设置
    /// </summary>
    public class WxOpenLoginDefaults
    {
        ///// <summary>
        ///// Configuration Section in appsettings.json
        ///// </summary>
        //public const string AppSettings = "OAuth:WeixinMiniProgram";
        /*
          "OAuth": {
              "WeixinMiniProgram": {
                "ClientId": "AppId",
                "ClientSecret": "Secret"
              }
          }
        */

        /// <summary>
        /// Default value for <see cref="AuthenticationOptions.DefaultAuthenticateScheme"/>.
        /// </summary>
        public const string AuthenticationScheme = "WeixinMiniProgram";

        /// <summary>
        /// Default value for <see cref="AuthenticationOptions.DefaultScheme"/>.
        /// </summary>
        public static readonly string DisplayName = "WeixinMiniProgram";

        /// <summary>
        /// Default value for <see cref="RemoteAuthenticationOptions.CallbackPath"/>.
        /// </summary>
        public const string CallbackPath = "/signin-weixin";

        /// <summary>
        /// Default value for <see cref="AuthenticationSchemeOptions.ClaimsIssuer"/>.
        /// </summary>
        public const string Issuer = "WeixinMiniProgram";

        /// <summary>
        /// 微信小程序服务端验证地址 for <see cref="WxOpenLoginHandler"/>.
        /// https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/login/auth.code2Session.html
        /// </summary>
        public static readonly string AuthorizationEndpoint = "https://api.weixin.qq.com/sns/jscode2session";
    }
}
