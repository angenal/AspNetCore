using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Authentication.WeChat
{
    /// <summary>
    /// Defines a set of options used by <see cref="WeChatAuthenticationHandler"/>.
    /// </summary>
    public class WeChatAuthenticationOptions : OAuthOptions
    {
        /// <summary></summary>
        public WeChatAuthenticationOptions()
        {
            ClaimsIssuer = WeChatAuthenticationDefaults.Issuer;
            CallbackPath = new PathString(WeChatAuthenticationDefaults.CallbackPath);

            AuthorizationEndpoint = WeChatAuthenticationDefaults.AuthorizationEndpoint;
            TokenEndpoint = WeChatAuthenticationDefaults.TokenEndpoint;
            UserInformationEndpoint = WeChatAuthenticationDefaults.UserInformationEndpoint;

            Scope.Add("snsapi_login");

            ClaimActions.MapJsonKey(ClaimTypes.Name, "nickname");
            ClaimActions.MapJsonKey(ClaimTypes.Gender, "sex");
            ClaimActions.MapJsonKey(ClaimTypes.Country, "country");

            ClaimActions.MapJsonKey("urn:weixin:nickname", "nickname");
            ClaimActions.MapJsonKey("urn:weixin:city", "city");
            ClaimActions.MapJsonKey("urn:weixin:province", "province");
            ClaimActions.MapJsonKey("urn:weixin:headimgurl", "headimgurl");
        }
    }
}
