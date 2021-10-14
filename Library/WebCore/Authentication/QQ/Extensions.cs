using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication.QQ
{
    /// <summary></summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        ///  Get the external login information from qq provider.
        /// </summary>
        public static async Task<Dictionary<string, string>> GetExternalQQLoginInfoAsync(this HttpContext httpContext, string expectedXsrf = null)
        {
            var auth = await httpContext.AuthenticateAsync(QQAuthenticationDefaults.AuthenticationScheme);

            var items = auth?.Properties?.Items;
            if (auth?.Principal == null || items == null || !items.ContainsKey("LoginProvider"))
            {
                return null;
            }

            if (expectedXsrf != null)
            {
                if (!items.ContainsKey("XsrfId"))
                {
                    return null;
                }
                var userId = items["XsrfId"];
                if (userId != expectedXsrf)
                {
                    return null;
                }
            }

            var userInfo = auth.Principal.FindFirst("urn:qq:user_info");
            if (userInfo == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(userInfo.Value))
            {
                return GetUserInfo(userInfo.Value);
            }

            return null;
        }

        private static Dictionary<string, string> GetUserInfo(string json)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            var jObject = JObject.Parse(json);

            foreach (var item in jObject)
            {
                dict[item.Key] = item.Value?.ToString();
            }
            return dict;
        }
    }
}
