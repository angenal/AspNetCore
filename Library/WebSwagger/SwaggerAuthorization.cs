using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;

namespace WebSwagger
{
    public static partial class SwaggerDocServices
    {
        /// <summary>
        /// 初始化Swagger身份验证
        /// </summary>
        private static void InitSwaggerSecurityDefinition()
        {
            string scheme = "ApiKey";
            var scheme1 = new OpenApiSecurityScheme()
            {
                Name = "X-API-KEY",
                Description = "API 密钥",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            };
            SecurityDefinitions.Add(scheme, scheme1);

            scheme = "Bearer";
            var scheme2 = new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Scheme = scheme,
                BearerFormat = "JWT",
                Description = "JWT 认证授权",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            };
            SecurityDefinitions.Add(scheme, scheme2);
        }

        /// <summary>
        /// 添加Swagger身份验证
        /// </summary>
        /// <param name="c"></param>
        private static void AddSwaggerSecurityDefinition(this SwaggerGenOptions c)
        {
            //c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>(){{"oauth2", new string[] { }}});

            string scheme = "ApiKey", queryName = "apiKey";
            c.AddSecurityDefinition(scheme, SecurityDefinitions[scheme]);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = scheme, Type = ReferenceType.SecurityScheme
                        },
                        Name = queryName, In = ParameterLocation.Query
                    },
                    Array.Empty<string>()
                }
            });

            scheme = "Bearer"; queryName = "token";
            c.AddSecurityDefinition(scheme, SecurityDefinitions[scheme]);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = scheme, Type = ReferenceType.SecurityScheme
                        },
                        Name = queryName, Scheme = scheme, In = ParameterLocation.Header
                    },
                    Array.Empty<string>()
                }
            });
        }

        /// <summary>
        /// Swagger使用localStorage存储Token
        /// </summary>
        /// <param name="c"></param>
        private static void UseSwaggerSecurityStorage(this SwaggerUIOptions c)
        {
            InitSwaggerSecurityDefinition();
            string scheme = "ApiKey";
            c.UseTokenStorage(scheme, SecurityDefinitions[scheme].ConvertToTDP(), WebCacheType.Local);
            scheme = "Bearer";
            c.UseTokenStorage(scheme, SecurityDefinitions[scheme].ConvertToTDP(), WebCacheType.Session);
        }
    }
}
