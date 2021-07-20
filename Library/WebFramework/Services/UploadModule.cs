using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using tusdotnet;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Models.Expiration;
using tusdotnet.Stores;

namespace WebFramework.Services
{
    /// <summary>
    /// Upload Module
    /// </summary>
    public static class UploadModule
    {
        static MinioOptions MinioOptions = new MinioOptions();
        const string MinioAppSettings = "Upload:Minio";
        /*
          "Upload": {
            "Minio": {
              "Endpoint": "play.min.io",
              "AccessKey": "Q3AM3UQ867SPQQA43P2F",
              "SecretKey": "zuf+tfteSlswRu7BJ86wekitnifILbZam1KYY3TG",
              "Region": "us-east-1",
              "SessionToken": ""
            }
          }
        */

        /// <summary>
        /// Register services
        /// </summary>
        public static IServiceCollection AddUpload(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            // 对象存储 Minio  https://min.io/download
            // 对象存储 Minio Docs  https://docs.min.io/docs/dotnet-client-quickstart-guide.html
            AddMinio(services, config);

            // 断点续传 Tus 默认配置
            services.AddSingleton(_ => TusFileUpload.CreateDefaultConfiguration(env));

            return services;
        }

        /// <summary>
        /// Use Upload Middleware
        /// </summary>
        public static IApplicationBuilder UseUpload(this IApplicationBuilder app)
        {
            // 断点续传 tusdotnet
            app.UseTus(httpContext => Task.FromResult(httpContext.RequestServices.GetService<DefaultTusConfiguration>()));

            return app;
        }

        /// <summary>
        /// 对象存储 Minio
        /// </summary>
        static void AddMinio(IServiceCollection services, IConfiguration config)
        {
            // Minio.AspNetCore  https://github.com/appany/Minio.AspNetCore
            // > dotnet add package Minio.AspNetCore

            var section = config.GetSection(MinioAppSettings);
            if (!section.Exists()) return;

            // Register IOptions<MinioOptions> from appsettings.json
            services.Configure<MinioOptions>(section);
            config.Bind(MinioAppSettings, MinioOptions);

            services.AddMinio(options =>
            {
                options.Endpoint = MinioOptions.Endpoint;
                options.AccessKey = MinioOptions.AccessKey;
                options.SecretKey = MinioOptions.SecretKey;
                options.Region = MinioOptions.Region;
                options.SessionToken = MinioOptions.SessionToken;
                options.OnClientConfiguration = client =>
                {
                    client.SetTraceOff(); // Sets HTTP tracing Off.
                    client.WithSSL(); // Connects to Cloud Storage with HTTPS if this method is invoked on client object.
                    client.WithTimeout(3600 * 1000); // Timeout in milliseconds.
                };
            });

            // Get or inject first minio client
            //var client = serviceProvider.GetRequiredService<MinioClient>();
        }


    }

    /// <summary>
    /// Tus File Upload  https://github.com/tus/tus-js-client
    /// </summary>
    public static class TusFileUpload
    {
        /// <summary>
        /// 上传目录
        /// </summary>
        public static string SavePath = "files";

        /// <summary>
        /// 下载目录
        /// </summary>
        public static string UrlPath = "/files";

        /// <summary>
        /// 断点续传 Tus 默认配置
        /// </summary>
        internal static DefaultTusConfiguration CreateDefaultConfiguration(IWebHostEnvironment env)
        {
            var path = Path.Combine(env.WebRootPath, SavePath);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return new DefaultTusConfiguration
            {
                UrlPath = UrlPath,
                //文件存储路径
                Store = new TusDiskStore(path),
                //元数据是否允许空值
                MetadataParsingStrategy = MetadataParsingStrategy.AllowEmptyValues,
                //文件过期后不再更新
                Expiration = new AbsoluteExpiration(TimeSpan.FromMinutes(10)),
                //事件处理
                Events = new Events
                {
                    //上传完成事件回调
                    OnFileCompleteAsync = async ctx =>
                    {
                        //获取上传文件
                        var file = await ctx.GetFileAsync();

                        //获取上传文件元数据
                        var meta = await file.GetMetadataAsync(ctx.CancellationToken);

                        //获取上述文件元数据中的目标文件名
                        var fileNameMeta = meta["name"];

                        //目标文件名以base64编码后需要解码
                        var fileName = fileNameMeta.GetString(Encoding.UTF8);

                        //将上传文件另存为实际目标文件
                        File.Move(Path.Combine(path, ctx.FileId), Path.Combine(path, ctx.FileId + Path.GetExtension(fileName)));
                    }
                }
            };
        }

        /// <summary>
        /// 下载文件 Tus Endpoint Handler
        /// </summary>
        public static async Task DownloadHandler(HttpContext context)
        {
            var config = context.RequestServices.GetRequiredService<DefaultTusConfiguration>();

            if (config.Store is not ITusReadableStore store)
            {
                return;
            }

            var fileId = context.Request.RouteValues["fileId"].ToString();
            var file = await store.GetFileAsync(fileId, context.RequestAborted);

            if (file == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync($"File with id {fileId} was not found.", context.RequestAborted);
                return;
            }

            var fileStream = await file.GetContentAsync(context.RequestAborted);
            var metadata = await file.GetMetadataAsync(context.RequestAborted);

            context.Response.ContentType = GetContentTypeOrDefault(metadata);
            context.Response.ContentLength = fileStream.Length;

            if (metadata.TryGetValue("name", out var nameMeta))
            {
                context.Response.Headers.Add("Content-Disposition", new[] { $"attachment; filename=\"{nameMeta.GetString(Encoding.UTF8)}\"" });
            }

            using (fileStream)
            {
                await fileStream.CopyToAsync(context.Response.Body, 81920, context.RequestAborted);
            }
        }

        static string GetContentTypeOrDefault(Dictionary<string, Metadata> metadata)
        {
            if (metadata.TryGetValue("contentType", out var contentType))
            {
                return contentType.GetString(Encoding.UTF8);
            }

            return "application/octet-stream";
        }
    }
}
