using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebFramework.Services
{
    /// <summary>
    /// Api Authentication + Authorization Module
    /// </summary>
    public static class AuthModule
    {
        /// <summary>
        /// Register Authentication + Authorization services
        /// </summary>
        public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            // Custom Api Authorization using WebFramework.Authorization
            //services.AddApiAuthorization(config);

            // Microsoft.AspNetCore.Identity system for the specified User and Role types
            //services.AddIdentityLiteDB(config);

            // Authentication with JWT
            var builder = services.AddJwtAuthentication(config);

            // Register Weixin Services
            services.AddWeixin(builder, config);

            // Authentication with OAuth
            if (config.GetSection("OAuth").Exists())
            {
                var oAuth = services.AddAuthentication();
                string qq = config.GetValue<string>("OAuth:QQ:ClientId"), qqSecret = config.GetValue<string>("OAuth:QQ:ClientSecret");
                string wx = config.GetValue<string>("OAuth:Weixin:ClientId"), wxSecret = config.GetValue<string>("OAuth:Weixin:ClientSecret");
                if (!string.IsNullOrEmpty(qq) && !string.IsNullOrEmpty(qqSecret)) oAuth.AddQQAuthentication(t =>
                {
                    t.ClientId = qq;
                    t.ClientSecret = qqSecret;
                });
                if (!string.IsNullOrEmpty(wx) && !string.IsNullOrEmpty(wxSecret)) oAuth.AddWeChatAuthentication(t =>
                {
                    t.ClientId = wx;
                    t.ClientSecret = wxSecret;
                });
            }


            // Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("test", policy => policy.RequireClaim("name", "测试"));
                options.AddPolicy("Upload", policy => policy.RequireAuthenticatedUser());
                options.AddPolicy("User", policy => policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "role" && c.Value.StartsWith("User")) ||
                    context.User.HasClaim(c => c.Type == "name" && c.Value.StartsWith("User"))));
            });

            return services;
        }
    }
}
