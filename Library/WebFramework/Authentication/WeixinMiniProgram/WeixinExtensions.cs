using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace WebFramework.Authentication.WeixinMiniProgram
{
    /// <summary></summary>
    public static class WeixinExtensions
    {
        /// <summary></summary>
        public static AuthenticationBuilder AddWeixinMiniProgram(this AuthenticationBuilder builder, Action<WeixinLoginOptions> configureOptions)
        {
            return builder.AddWeixinMiniProgram(WeixinLoginDefaults.AuthenticationScheme, configureOptions);
        }
        /// <summary></summary>
        public static AuthenticationBuilder AddWeixinMiniProgram(this AuthenticationBuilder builder, string authenticationScheme, Action<WeixinLoginOptions> configureOptions)
        {
            return builder.AddWeixinMiniProgram(authenticationScheme, WeixinLoginDefaults.DisplayName, configureOptions);
        }
        /// <summary></summary>
        public static AuthenticationBuilder AddWeixinMiniProgram(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<WeixinLoginOptions> configureOptions)
        {
            builder.Services.TryAddSingleton<IWeixinLoginStateInfoStore, WeixinLoginStateInfoStore>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<WeixinLoginOptions>, WeixinPostConfigureOptions>());
            return builder.AddRemoteScheme<WeixinLoginOptions, WeixinLoginHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
