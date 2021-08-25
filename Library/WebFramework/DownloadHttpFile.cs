using Downloader;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using WebCore;

namespace WebFramework
{
    /// <summary>
    /// 下载 HTTP 协议的文件
    /// </summary>
    public class DownloadHttpFile
    {
        /// <summary></summary>
        public static async Task<string> Download(string url, string file = null, HttpMethod httpMethod = null,
            NameValueCollection headers = null,
            string userAgent = DefaultFormat.UserAgent)
        {
            var http = new HttpClient();
            if (httpMethod == null) httpMethod = HttpMethod.Get;
            var request = new HttpRequestMessage(httpMethod, url);
            if (headers != null)
            {
                foreach (string key in headers.Keys) request.Headers.Add(key, headers[key]);
            }
            if (userAgent != null)
            {
                request.Headers.Add("User-Agent", userAgent);
            }
            var response = await http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            if (string.IsNullOrEmpty(file)) file = Path.GetTempFileName();
            using (var fs = File.Open(file, FileMode.Create))
            {
                using var ms = response.Content.ReadAsStream(); await ms.CopyToAsync(fs);
            }
            return file;
        }

        /// <summary></summary>
        public static async Task Download(string url, string file,
            Action<object, DownloadStartedEventArgs> downloadStarted,
            Action<object, AsyncCompletedEventArgs> downloadFileCompleted,
            Action<object, Downloader.DownloadProgressChangedEventArgs> chunkDownloadProgressChanged = null,
            Action<object, Downloader.DownloadProgressChangedEventArgs> downloadProgressChanged = null,
            NameValueCollection headers = null, Cookie[] cookies = null,
            string userAgent = DefaultFormat.UserAgent)
        {
            var options = GetDownloadConfiguration(headers, cookies, userAgent);

            var downloader = new DownloadService(options);

            // 在每次下载开始时提供 文件名 和 要接收的总字节数
            downloader.DownloadStarted += (object sender, DownloadStartedEventArgs e) => downloadStarted?.Invoke(sender, e);
            // 提供有关分块下载的信息，如每个分块的进度百分比、速度、收到的总字节数和收到的字节数组，以实现实时流
            downloader.ChunkDownloadProgressChanged += (object sender, Downloader.DownloadProgressChangedEventArgs e) => chunkDownloadProgressChanged?.Invoke(sender, e);
            // 提供任何关于下载进度的信息，如进度百分比的块数总和、总速度、平均速度、总接收字节数和接收字节数组的实时流
            downloader.DownloadProgressChanged += (object sender, Downloader.DownloadProgressChangedEventArgs e) => downloadProgressChanged?.Invoke(sender, e);
            // 下载完成的事件，可以包括发生错误或被取消或下载成功
            downloader.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) => downloadFileCompleted?.Invoke(sender, e);

            await downloader.DownloadFileTaskAsync(url, file);
        }

        /// <summary></summary>
        public static DownloadConfiguration GetDownloadConfiguration(NameValueCollection headers = null, Cookie[] cookies = null, string userAgent = DefaultFormat.UserAgent)
        {
            var config = new DownloadConfiguration()
            {
                BufferBlockSize = 8 * 1024, // 通常，主机最大支持8000字节，默认值为8000
                ChunkCount = Environment.ProcessorCount, // 要下载的文件分片数量，默认值为1
                MaximumBytesPerSecond = 1024 * 1024, // 下载速度限制为1MB/s，默认值为零或无限制
                MaxTryAgainOnFailover = 10, // 失败的最大次数
                OnTheFlyDownload = false, // 是否在内存中进行缓存？ 默认值是true
                ParallelDownload = true, // 下载文件是否为并行的。默认值为false
                TempDirectory = Path.GetTempPath(), // 设置用于缓冲区文件的临时路径，默认为Path.GetTempPath()
                Timeout = 1000, // 每个StreamReader的超时(毫秒)，默认值是1000
                RequestConfiguration = // 定制请求头
                {
                    Accept = "*/*",
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    Headers = new WebHeaderCollection(),
                    CookieContainer = new CookieContainer(),
                    ProtocolVersion = HttpVersion.Version11,
                    UserAgent = userAgent ?? $"Downloader {Assembly.GetEntryAssembly().GetName().Version.ToString(3)}",
                    UseDefaultCredentials = false,
                    KeepAlive = false,
                }
            };
            if (headers != null)
            {
                foreach (string key in headers.Keys) config.RequestConfiguration.Headers.Add(key, headers[key]);
            }
            if (cookies != null)
            {
                foreach (Cookie cookie in cookies) config.RequestConfiguration.CookieContainer.Add(cookie);
            }
            return config;
        }
    }
}
