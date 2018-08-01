using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace WebFramework.Authentications
{
    /// <summary>
    /// JWT认证: /api/auth/token
    /// </summary>
    public class JwtToken
    {
        public JwtToken(IConfiguration configuration)
        {
            Configuration = configuration;
            HOST = Configuration["HOST"] ?? "api.com";
            AUTH = Configuration["Authorization"] ?? "Authorization";
            KEY = Configuration["KEY"] ?? "f2355e3bb049eaa89e4e07a055042e9793b947cd49d09537fbb72e6e1f9a80e2f2355e3bb049eaa89e4e07a055042e9793b947cd49d09537fbb72e6e1f9a80e2";
        }

        /// <summary>
        /// 获取新的Token
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <param name="expires"></param>
        /// <returns></returns>
        public async Task<SecurityToken> NewToken(ControllerBase controller, Func<string, bool> action, DateTime? expires)
        {
            return await Task.Run<SecurityToken>(() =>
            {
                var auth = controller.Request.Headers[AUTH];
                if (!StringValues.IsNullOrEmpty(auth) && auth.ToString().StartsWith("Basic"))
                {
                    var authValue = auth.ToString().Substring(5).Trim();
                    if (!authValue.Contains(':')) // authValue(base64): admin:admin
                    {
                        authValue = Encoding.UTF8.GetString(Convert.FromBase64String(authValue));
                        string name = authValue.Split(':')[0];
                        if (action.Invoke(authValue))
                        {
                            var host = HOST ?? controller.Request.Host.Host;
                            var claims = new[] { new Claim(ClaimTypes.Name, name) };
                            var key = IssuerSigningKey();
                            var sign = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
                            var token = new JwtSecurityToken(
                                audience: host,
                                claims: claims,
                                expires: expires ?? DateTime.Now.AddMinutes(30),
                                issuer: host,
                                signingCredentials: sign
                                );
                            return token;
                        }
                    }
                }
                return null;
            });
        }

        #region internal var
        internal SecurityKey IssuerSigningKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
        }

        internal IConfiguration Configuration { get; }
        internal string AUTH { get; }
        internal string HOST { get; }
        internal string KEY { get; }
        #endregion
    }

    /// <summary>
    /// 依赖注入 services.AddAuthenticationWithJwt 服务
    /// </summary>
    public static class JwtTokenExtensions
    {
        /// <summary>
        /// Add Authentication With Jwt
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddAuthenticationWithJwt(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                var token = new JwtToken(configuration);
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = token.HOST,
                    ValidAudience = token.HOST,
                    IssuerSigningKey = token.IssuerSigningKey()
                };
            });
        }

        public static string ToTokenString(this SecurityToken token)
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(token);
        }
    }
}
