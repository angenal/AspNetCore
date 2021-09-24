using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication.WxOpen
{
    /// <summary>
    /// 微信小程序远程服务器身份验证的生命周期事件
    /// </summary>
    public class WxOpenLoginEvents : RemoteAuthenticationEvents
    {
        /// <summary>
        /// 微信服务端验证完成后触发,注册该方法获取用户信息.
        /// </summary>
        public Func<WxOpenServerResultContext, Task> OnWxOpenServerCompleted { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// 微信服务端验证完成后将会调用该方法.
        /// </summary>
        public virtual Task WxOpenServerCompleted(WxOpenServerResultContext context) => OnWxOpenServerCompleted?.Invoke(context);
    }
}
