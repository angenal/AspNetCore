using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using WebFramework.Authentication.WeChat.WxOpen;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary></summary>
    public static class WxOpenExtensions
    {
        /// <summary></summary>
        public static AuthenticationBuilder AddWeixinMiniProgramAuthentication(this AuthenticationBuilder builder, Action<WxOpenLoginOptions> configureOptions)
        {
            return builder.AddWeixinMiniProgramAuthentication(WxOpenLoginDefaults.AuthenticationScheme, configureOptions);
        }
        /// <summary></summary>
        public static AuthenticationBuilder AddWeixinMiniProgramAuthentication(this AuthenticationBuilder builder, string authenticationScheme, Action<WxOpenLoginOptions> configureOptions)
        {
            return builder.AddWeixinMiniProgramAuthentication(authenticationScheme, WxOpenLoginDefaults.DisplayName, configureOptions);
        }
        /// <summary></summary>
        public static AuthenticationBuilder AddWeixinMiniProgramAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<WxOpenLoginOptions> configureOptions)
        {
            builder.Services.TryAddSingleton<IWxOpenLoginStateInfoStore, WxOpenLoginStateInfoStore>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<WxOpenLoginOptions>, WxOpenPostConfigureOptions>());
            return builder.AddRemoteScheme<WxOpenLoginOptions, WxOpenLoginHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
