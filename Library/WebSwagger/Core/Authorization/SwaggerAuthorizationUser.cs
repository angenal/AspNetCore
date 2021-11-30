using WebSwagger.Internals;

namespace WebSwagger.Core.Authorization
{
    /// <summary>
    /// Swagger授权用户
    /// </summary>
    public class SwaggerAuthorizationUser
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 授权令牌
        /// </summary>
        public string Token => Encrypt.HmacSha256($"{UserName}{Password}", SecretKey);

        /// <summary>
        /// 授权令牌的生成密钥
        /// </summary>
        public static string SecretKey = "WebSwagger";

        /// <summary>
        /// 初始化一个<see cref="SwaggerAuthorizationUser"/>类型的实例
        /// </summary>
        public SwaggerAuthorizationUser() { }

        /// <summary>
        /// 初始化一个<see cref="SwaggerAuthorizationUser"/>类型的实例
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        public SwaggerAuthorizationUser(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }
}
