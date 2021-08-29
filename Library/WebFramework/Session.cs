using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using WebCore;
using WebInterface.Settings;

namespace WebFramework
{
    /// <summary>
    /// 用户会话 by Jwt Token
    /// </summary>
    public sealed class Session : IdentityUser<string>
    {
        /// <summary>
        /// 用户会话
        /// </summary>
        public Session() : base() { }
        /// <summary>
        /// 用户会话
        /// </summary>
        /// <param name="userName"></param>
        public Session(string userName) : base(userName) { }
        /// <summary>
        /// 用户会话
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <param name="userName">唯一账号</param>
        public Session(string id, string userName) : base(userName) => Id = id;

        /// <summary>
        /// 类别
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 别名
        /// </summary>
        public string Nickname { get; set; }
        /// <summary>
        /// 身份证号码
        /// </summary>
        public string IdCard { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }
    }

    /// <summary>
    /// 用户会话帮助 by Jwt Token
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// 转换登录身份信息为当前会话信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static Session Session(this ClaimsPrincipal user)
        {
            if (user == null) return null;

            var session = new Session(user.FindFirstValue(JwtSettings.NameClaimType), user.FindFirstValue(JwtRegisteredClaimNames.Sub));

            var roles = user.FindAll(t => t.Type.Equals(JwtSettings.RoleClaimType)).Select(t => t.Value);
            session.Role = string.Join(',', roles);

            session.Type = user.FindFirstValue("type");

            session.Name = user.FindFirstValue("name");
            session.Nickname = user.FindFirstValue("nickname");
            session.IdCard = user.FindFirstValue("idcard"); // to login
            session.Avatar = user.FindFirstValue("avatar");
            session.NormalizedUserName = ""; // If IdCard confirmed, will be has value

            session.PhoneNumber = user.FindFirstValue("phone"); // to login
            session.PhoneNumberConfirmed = false;
            session.TwoFactorEnabled = false; // If enabled, SMS verification will be sent

            session.Email = user.FindFirstValue("email"); // to login
            session.EmailConfirmed = false; // If enabled, email verification will be sent
            session.NormalizedEmail = ""; // If email confirmed, will be has value

            session.PasswordHash = ""; // If you use a one-time password: OTP
            session.ConcurrencyStamp = user.FindFirstValue("cs"); // A random value
            session.SecurityStamp = user.FindFirstValue("ss"); // A random value

            session.LockoutEnabled = false; // Lock user and disable login
            session.AccessFailedCount = 0; // the number of failed login attempts for the current user

            return session;
        }
        /// <summary>
        /// 转换当前会话信息为登录身份信息
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static IEnumerable<Claim> Claims(this Session session)
        {
            if (session != null)
            {
                yield return new Claim(JwtSettings.NameClaimType, session.Id.ToString());
                yield return new Claim(JwtRegisteredClaimNames.Sub, session.UserName);

                string[] roles = (session.Role ??= "").Split(',');
                foreach (string role in roles) yield return new Claim(JwtSettings.RoleClaimType, role.Trim());

                yield return new Claim("type", session.Type ??= "");

                yield return new Claim("name", session.Name ??= "");
                yield return new Claim("nickname", session.Nickname ??= "");
                yield return new Claim("idcard", session.IdCard ??= "");
                yield return new Claim("avatar", session.Avatar ??= "");

                yield return new Claim("phone", session.PhoneNumber ??= "");

                yield return new Claim("email", session.Email ??= "");

                yield return new Claim("cs", session.ConcurrencyStamp ??= "");
                yield return new Claim("ss", session.SecurityStamp ??= DateTime.Now.x8());
            }
        }
    }
}
