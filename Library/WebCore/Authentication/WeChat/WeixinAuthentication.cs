using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.WeChat;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary></summary>
    public static class WeChatAuthenticationExtensions
    {
        /// <summary></summary>
        public static AuthenticationBuilder AddWeixinAuthentication(this AuthenticationBuilder builder)
        {
            return builder.AddWeixinAuthentication(WeChatAuthenticationDefaults.AuthenticationScheme, WeChatAuthenticationDefaults.DisplayName, options => { });
        }
        /// <summary></summary>
        public static AuthenticationBuilder AddWeixinAuthentication(this AuthenticationBuilder builder, Action<WeChatAuthenticationOptions> configureOptions)
        {
            return builder.AddWeixinAuthentication(WeChatAuthenticationDefaults.AuthenticationScheme, WeChatAuthenticationDefaults.DisplayName, configureOptions);
        }
        /// <summary></summary>
        public static AuthenticationBuilder AddWeixinAuthentication(this AuthenticationBuilder builder, string authenticationScheme, Action<WeChatAuthenticationOptions> configureOptions)
        {
            return builder.AddWeixinAuthentication(authenticationScheme, WeChatAuthenticationDefaults.DisplayName, configureOptions);
        }
        /// <summary></summary>
        public static AuthenticationBuilder AddWeixinAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<WeChatAuthenticationOptions> configureOptions)
        {
            return builder.AddOAuth<WeChatAuthenticationOptions, WeChatAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
