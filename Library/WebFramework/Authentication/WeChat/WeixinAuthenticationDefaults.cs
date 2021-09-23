namespace Microsoft.AspNetCore.Authentication.WeChat
{
	/// <summary>
	/// Default values for WeChat authentication.
	/// </summary>
	internal static class WeChatAuthenticationDefaults
	{
		/// <summary>
		/// Default value for <see cref="AuthenticationOptions.DefaultAuthenticateScheme"/>.
		/// </summary>
		public const string AuthenticationScheme = "WeChat";

		public const string DisplayName = "WeChat";

		/// <summary>
		/// Default value for <see cref="RemoteAuthenticationOptions.CallbackPath"/>.
		/// </summary>
		public const string CallbackPath = "/signin-WeChat";

		/// <summary>
		/// Default value for <see cref="AuthenticationSchemeOptions.ClaimsIssuer"/>.
		/// </summary>
		public const string Issuer = "WeChat";

		/// <summary>
		/// Default value for <see cref="OAuth.OAuthOptions.AuthorizationEndpoint"/>.
		/// </summary>
		public const string AuthorizationEndpoint = "https://open.WeChat.qq.com/connect/qrconnect";

		/// <summary>
		/// Default value for <see cref="OAuth.OAuthOptions.TokenEndpoint"/>.
		/// </summary>
		public const string TokenEndpoint = "https://api.WeChat.qq.com/sns/oauth2/access_token";

		/// <summary>
		/// Default value for <see cref="OAuth.OAuthOptions.UserInformationEndpoint"/>.
		/// </summary>
		public const string UserInformationEndpoint = "https://api.WeChat.qq.com/sns/userinfo";
	}
}