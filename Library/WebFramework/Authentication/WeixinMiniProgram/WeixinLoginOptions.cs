using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace WebFramework.Authentication.WeixinMiniProgram
{
    public class WeixinLoginOptions : RemoteAuthenticationOptions
    {
        /// <summary>
        /// 微信小程序 appId【请注意该信息的安全性,避免下发至客户端】
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 微信小程序 appSecret【请注意该信息的安全性,避免下发至客户端】
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// 微信授权类型, 默认: authorization_code
        /// </summary>
        public string GrantType { get; } = "authorization_code";

        /// <summary>
        /// 从微信服务器换取用户信息, 携带小程序客户端获取到的参数, 默认: code
        /// 
        /// <para>
        ///     该值需要配合参数 CallbackPath, 默认: SigninWeixinMiniProgram
        ///     则"https://localhost/SigninWeixinMiniProgram?code={code}"为验证地址,而{code}则会被传递至微信服务器进行验证.
        /// </para>
        /// </summary>
        public string JsQuery { get; set; } = "code";

        /// <summary>
        /// 根据微信服务器返回的会话密匙执行登录操作, 比如颁发JWT, 缓存OpenId, 重定向Action等.
        /// </summary>
        public Func<WeixinLoginStateContext, Task> CustomerLoginState { get; set; }

        /// <summary>
        /// 缓存过期时间, 默认：1天
        /// </summary>
        public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromDays(1);

        /// <summary>
        /// Gets or sets the <see cref="WeixinLoginEvents"/> used to handle authentication events.
        /// </summary>
        public new WeixinLoginEvents Events
        {
            get => (WeixinLoginEvents)base.Events;
            set => base.Events = value;
        }

        /// <summary></summary>
        public WeixinLoginOptions()
        {
            BackchannelTimeout = TimeSpan.FromSeconds(60);
            CallbackPath = new PathString("/SigninWeixinMiniProgram");
            Events = new WeixinLoginEvents();
        }

        /// <summary></summary>
        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(AppId))
                throw new ArgumentException($"微信小程序 {nameof(AppId)} 不能为空");

            if (string.IsNullOrEmpty(Secret))
                throw new ArgumentException($"微信小程序 {nameof(Secret)} 不能为空");
        }
    }
}
