namespace WebFramework.Authentication.WeChat.WxOpen
{
    /// <summary>
    /// 微信小程序登录校验的默认设置
    /// </summary>
    public class WxOpenLoginDefaults
    {
        /// <summary>
        /// Configuration Section in appsettings.json
        /// </summary>
        public const string AppSettings = "WeChatMiniProgram";
        /*
          "WeChatMiniProgram": {
            "appid": "",
            "secret": ""
          }
        */

        /// <summary></summary>
        public const string AuthenticationScheme = "MiniProgam,WeChat";

        /// <summary></summary>
        public static readonly string DisplayName = "WeChatMiniProgram";

        /// <summary>
        /// 微信小程序服务端验证地址
        /// https://developers.weixin.qq.com/miniprogram/dev/api-backend/open-api/login/auth.code2Session.html
        /// </summary>
        public static readonly string AuthorizationEndpoint = "https://api.weixin.qq.com/sns/jscode2session";
    }
}
