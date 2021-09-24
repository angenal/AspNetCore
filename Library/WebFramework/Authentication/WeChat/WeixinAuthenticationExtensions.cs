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

#if NETSTANDARD2_0

#region Old

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods to add weixin authentication capabilities to an HTTP application pipeline.
    /// </summary>
    public static class WeChatAuthenticationExtensions
    {
        /// <summary>
        /// Adds the <see cref="WeChatAuthenticationMiddleware"/> middleware to the specified
        /// <see cref="IApplicationBuilder"/>, which enables weixin authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="options">A <see cref="WeChatAuthenticationOptions"/> that specifies options for the middleware.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseWeChatAuthentication(this IApplicationBuilder app, WeChatAuthenticationOptions options)
        {
            throw new NotSupportedException("This method is no longer supported, see https://go.microsoft.com/fwlink/?linkid=845470");
        }

        /// <summary>
        /// Adds the <see cref="WeChatAuthenticationMiddleware"/> middleware to the specified
        /// <see cref="IApplicationBuilder"/>, which enables weixin authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="configuration">An action delegate to configure the provided <see cref="WeChatAuthenticationOptions"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseWeChatAuthentication(this IApplicationBuilder app, Action<WeChatAuthenticationOptions> configuration)
        {
            throw new NotSupportedException("This method is no longer supported, see https://go.microsoft.com/fwlink/?linkid=845470");
        }
    }

}

#endregion

#endif
