using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.QQ;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// </summary>
    public static class QQAuthenticationExtensions
    {
        /// <summary>
        /// </summary>
        public static AuthenticationBuilder AddQQAuthentication(this AuthenticationBuilder builder)
        {
            return builder.AddQQAuthentication(QQAuthenticationDefaults.AuthenticationScheme, QQAuthenticationDefaults.DisplayName, options => { });
        }

        /// <summary>
        /// </summary>
        public static AuthenticationBuilder AddQQAuthentication(this AuthenticationBuilder builder, Action<QQAuthenticationOptions> configureOptions)
        {
            return builder.AddQQAuthentication(QQAuthenticationDefaults.AuthenticationScheme, QQAuthenticationDefaults.DisplayName, configureOptions);
        }

        /// <summary>
        /// </summary>
        public static AuthenticationBuilder AddQQAuthentication(this AuthenticationBuilder builder, string authenticationScheme, Action<QQAuthenticationOptions> configureOptions)
        {
            return builder.AddQQAuthentication(authenticationScheme, QQAuthenticationDefaults.DisplayName, configureOptions);
        }

        /// <summary>
        /// </summary>
        public static AuthenticationBuilder AddQQAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<QQAuthenticationOptions> configureOptions)
        {
            return builder.AddOAuth<QQAuthenticationOptions, QQAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
