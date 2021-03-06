using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.Authentication.WeChat
{
    /// <summary>
    /// Contains static methods that allow to extract user's information from a <see cref="JObject"/>
    /// instance retrieved from weixin after a successful authentication process.
    /// </summary>
    static class WeChatAuthenticationHelper
    {
        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        public static string GetOpenId(JObject user) => user.Value<string>("openid");

        /// <summary>
        /// Gets the nickname associated with the user profile.
        /// </summary>
        public static string GetNickname(JObject user) => user.Value<string>("nickname");

        /// <summary>
        /// Gets the gender associated with the user profile.
        /// </summary>
        public static string GetSex(JObject user) => user.Value<string>("sex");

        /// <summary>
        /// Gets the province associated with the user profile.
        /// </summary>
        public static string GetProvince(JObject user) => user.Value<string>("province");

        /// <summary>
        /// Gets the city associated with the user profile.
        /// </summary>
        public static string GetCity(JObject user) => user.Value<string>("city");

        /// <summary>
        /// Gets the country associated with the user profile.
        /// </summary>
        public static string GetCountry(JObject user) => user.Value<string>("country");

        /// <summary>
        /// Gets the avatar image url associated with the user profile.
        /// </summary>
        public static string GetHeadimgUrl(JObject user) => user.Value<string>("headimgurl");

        /// <summary>
        /// Gets the union id associated with the application.
        /// </summary>
        public static string GetUnionid(JObject user) => user.Value<string>("unionid");

        /// <summary>
        /// Gets the privilege associated with the user profile.
        /// </summary>
        public static string GetPrivilege(JObject user)
        {
            var value = user.Value<JArray>("privilege");
            return value == null ? null : string.Join(",", value.ToObject<string[]>());
        }
    }
}
