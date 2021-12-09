using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebSwaggerDemo.NET5.Common
{
    /// <summary>
    /// JWT authentication service.
    /// </summary>
    public static class JwtAuthenticationService
    {
        /// <summary>
        /// Register JWT token authentication service.
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var section = config.GetSection(JwtSettings.AppSettings);
            if (!section.Exists()) return services;

            // Register IOptions<JwtSettings> from appsettings.json
            services.Configure<JwtSettings>(section);
            // Read settings
            config.Bind(JwtSettings.AppSettings, JwtGenerator.Settings);
            // Read environment variable from the current process
            string secretKey = Environment.GetEnvironmentVariable(JwtSettings.EnvSecretKey), encryptionKey = Environment.GetEnvironmentVariable(JwtSettings.EnvEncryptionKey);
            if (!string.IsNullOrEmpty(secretKey)) JwtGenerator.Settings.SecretKey = secretKey;
            if (!string.IsNullOrEmpty(encryptionKey)) JwtGenerator.Settings.EncryptionKey = encryptionKey;

            // Remove default claims
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddSingleton<IJwtGenerator, JwtGenerator>().AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = string.IsNullOrEmpty(JwtGenerator.Settings.EncryptionKey)
                // Without encryption
                ? new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = JwtGenerator.Settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = JwtGenerator.Settings.Audience,
                    ValidateIssuerSigningKey = true, // Token signature will be verified using a secret key.
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtGenerator.Settings.SecretKey)),
                    ValidateLifetime = true, // Token will only be valid if not expired yet, default 5 minutes clock skew.
                    NameClaimType = JwtSettings.NameClaimType,
                    RoleClaimType = JwtSettings.RoleClaimType,
                    ClockSkew = JwtGenerator.Settings.ClockSkew
                }
                // With encryption
                : new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true, // Token signature will be verified using a private key.
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtGenerator.Settings.SecretKey)),
                    TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtGenerator.Settings.EncryptionKey)),
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true, // Token will only be valid if not expired yet, default 5 minutes clock skew.
                    NameClaimType = JwtSettings.NameClaimType,
                    RoleClaimType = JwtSettings.RoleClaimType,
                    ClockSkew = JwtGenerator.Settings.ClockSkew
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query.ContainsKey("access_token") ? context.Request.Query["access_token"] : context.Request.Query[JwtSettings.HttpRequestQuery];
                        if (string.IsNullOrEmpty(accessToken)) return Task.CompletedTask;
                        if (!string.IsNullOrEmpty(JwtGenerator.Settings.EncryptionKey))
                        {
                        }
                        context.Token = accessToken;
                        //context.HttpContext.Request.Headers.Add("X-Request-Uid", context.Request.Query["uid"]); // UserID
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }

    /// <summary>
    /// JWT generator and appsettings.
    /// </summary>
    public class JwtGenerator : IJwtGenerator
    {
        /// <summary>
        /// JWT settings.
        /// </summary>
        public static JwtSettings Settings = new JwtSettings();

        /// <summary>
        /// JWT token generator.
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        public string Generate(Func<IEnumerable<Claim>> generator) => Generate(() => generator());
        /// <summary></summary>
        public string Generate(IEnumerable<Claim> claim)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken token;
            var nbf = DateTime.UtcNow;
            var exp = nbf.AddMinutes(Convert.ToDouble(Settings.ExpireMinutes));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //new Claim(JwtRegisteredClaimNames.Sid, data["userid"].ToString()),
                //new Claim(JwtRegisteredClaimNames.Sub, data["username"].ToString()),
            };

            if (claim != null)
            {
                claims.AddRange(claim);
            }

            // Without encryption
            if (string.IsNullOrEmpty(Settings.EncryptionKey))
            {
                var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.SecretKey)), SecurityAlgorithms.HmacSha256);
                token = new JwtSecurityToken(Settings.Issuer, Settings.Audience, claims, nbf, exp, creds);
            }
            // With encryption
            else
            {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    IssuedAt = nbf,
                    Expires = exp,
                    Subject = new ClaimsIdentity(claims),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.SecretKey)), SecurityAlgorithms.HmacSha256Signature),
                    EncryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.EncryptionKey)), SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256)
                };
                token = tokenHandler.CreateToken(tokenDescriptor);
            }

            return tokenHandler.WriteToken(token);
        }
    }

    /// <summary>
    /// JWT generator interface.
    /// </summary>
    public interface IJwtGenerator
    {
        /// <summary></summary>
        string Generate(IEnumerable<Claim> claims);
        /// <summary></summary>
        string Generate(Func<IEnumerable<Claim>> generator);
    }

    /// <summary>
    /// JWT settings.
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Default Instance.
        /// </summary>
        public static JwtSettings Instance = new JwtSettings();
        /// <summary>
        /// Configuration Section in appsettings.json
        /// </summary>
        public const string AppSettings = "JWT";
        /*
          "JWT": {
            "Issuer": "localhost",
            "Audience": "localhost",
            "SecretKey": "g9MiOLxxpcwUJJkbkzzr766137NkLNdUsawpF4uIKyo=",
            "EncryptionKey": "g9MiOLxxpcwUJJkb",
            "ExpireMinutes": 720,
            "ClockSkew": "00:05:00"
          }
        */

        /// <summary>
        /// EnvironmentVariable: SecretKey
        /// </summary>
        public const string EnvSecretKey = "JWT_SecretKey";
        /// <summary>
        /// EnvironmentVariable: EncryptionKey
        /// </summary>
        public const string EnvEncryptionKey = "JWT_EncryptionKey";
        /// <summary>
        /// HttpRequest Query Variable
        /// </summary>
        public const string HttpRequestQuery = "token";

        /// <summary>
        /// Gets or sets a System.String that defines the System.Security.Claims.ClaimsIdentity.NameClaimType.
        /// </summary>
        public const string NameClaimType = "sid";
        /// <summary>
        /// Gets or sets the System.String that defines the System.Security.Claims.ClaimsIdentity.RoleClaimType.
        /// </summary>
        public const string RoleClaimType = "role";

        /// <summary>
        /// 谁颁发?
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// 哪些客户端用?
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// 签名密钥?
        /// </summary>
        public string SecretKey { get; set; }
        /// <summary>
        /// 加密密钥?
        /// </summary>
        public string EncryptionKey { get; set; }
        /// <summary>
        /// 过期(分钟数)?
        /// </summary>
        public int ExpireMinutes { get; set; }
        /// <summary>
        /// 时钟偏移(默认5分钟"00:05:00")
        /// </summary>
        [DefaultValue(300)]
        public TimeSpan ClockSkew { get; set; } = new TimeSpan(0, 5, 0);
    }
}
