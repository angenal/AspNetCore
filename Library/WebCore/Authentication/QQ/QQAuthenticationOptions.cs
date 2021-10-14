using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Authentication.QQ
{
    /// <summary>
    /// </summary>
    public class QQAuthenticationOptions : OAuthOptions
    {
        /// <summary></summary>
        public string OpenIdEndpoint { get; set; }

        /// <summary></summary>
        public QQAuthenticationOptions()
        {
            ClaimsIssuer = QQAuthenticationDefaults.Issuer;
            CallbackPath = new PathString(QQAuthenticationDefaults.CallbackPath);

            AuthorizationEndpoint = QQAuthenticationDefaults.AuthorizationEndpoint;
            TokenEndpoint = QQAuthenticationDefaults.TokenEndpoint;
            UserInformationEndpoint = QQAuthenticationDefaults.UserInformationEndpoint;
            OpenIdEndpoint = QQAuthenticationDefaults.UserOpenIdEndpoint;

            ClaimActionCollectionMapExtensions.MapJsonKey(ClaimActions, ClaimTypes.NameIdentifier, "id");
            ClaimActionCollectionMapExtensions.MapJsonKey(ClaimActions, ClaimTypes.Name, "displayName");
            //ClaimActionCollectionMapExtensions.MapJsonKey(ClaimActions, ClaimTypes.Name, "nickname");
            ClaimActionCollectionMapExtensions.MapJsonKey(ClaimActions, ClaimTypes.Gender, "gender");
            ClaimActionCollectionMapExtensions.MapJsonKey(ClaimActions, "urn:qq:figureurl", "figureurl");
            ClaimActionCollectionMapExtensions.MapJsonKey(ClaimActions, "urn:qq:figureurl_1", "figureurl_1");
            ClaimActionCollectionMapExtensions.MapJsonKey(ClaimActions, "urn:qq:figureurl_2", "figureurl_2");
            ClaimActionCollectionMapExtensions.MapJsonKey(ClaimActions, "urn:qq:figureurl_qq_1", "figureurl_qq_1");
            ClaimActionCollectionMapExtensions.MapJsonKey(ClaimActions, "urn:qq:figureurl_qq_2", "figureurl_qq_2");
            ClaimActionCollectionMapExtensions.MapJsonKey(ClaimActions, "urn:qq:is_yellow_vip", "is_yellow_vip");
            ClaimActionCollectionMapExtensions.MapJsonKey(ClaimActions, "urn:qq:vip", "vip");
            ClaimActionCollectionMapExtensions.MapJsonKey(ClaimActions, "urn:qq:yellow_vip_level", "yellow_vip_level");
            ClaimActionCollectionMapExtensions.MapJsonKey(ClaimActions, "urn:qq:level", "level");
            ClaimActionCollectionMapExtensions.MapJsonKey(ClaimActions, "urn:qq:is_yellow_year_vip", "is_yellow_year_vip");
        }
    }
}
