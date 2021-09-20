using Microsoft.AspNetCore.Authentication;
using System;
using System.Threading.Tasks;

namespace WebFramework.Authentication.WeixinMiniProgram
{
    /// <summary>
    /// 微信小程序身份验证的生命周期事件
    /// </summary>
    public class WeixinLoginEvents : RemoteAuthenticationEvents
    {
        /// <summary>
        /// 微信服务端验证完成后触发,注册该方法获取用户信息.
        /// </summary>
        public Func<WeixinServerResultContext, Task> OnWeixinServerCompleted { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// 微信服务端验证完成后将会调用该方法.
        /// </summary>
        public virtual Task WeixinServerCompleted(WeixinServerResultContext context) => OnWeixinServerCompleted(context);
    }
}
