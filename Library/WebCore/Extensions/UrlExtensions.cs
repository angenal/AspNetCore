using Microsoft.AspNetCore.Http;
using System;

namespace WebCore
{
    public static class UrlExtensions
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string RootPath(this Uri uri) => uri.Scheme + "://" + uri.Authority + "/";
        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string RootPath(this HttpRequest request) => request.Scheme + "://" + request.Host + "/";
        /// <summary>
        ///
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsUrl(this string url) => !string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out _);
        /// <summary>
        ///
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsHttpUrl(this string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            if (url.StartsWith("http://") || url.StartsWith("https://"))
                return Uri.TryCreate(url, UriKind.Absolute, out _);
            return false;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="url"></param>
        /// <param name="uriOut"></param>
        /// <returns></returns>
        public static bool IsSameDomainUrl(this Uri uri, string url, out Uri uriOut)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri2) && uri2.Authority.Equals(uri.Authority, StringComparison.OrdinalIgnoreCase))
            {
                uriOut = uri2;
                return true;
            }
            uriOut = new Uri("http://x");
            return false;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool TryGetUrlPath(this Uri uri, string url, out string path)
        {
            path = null;
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri2) && uri2.Authority.Equals(uri.Authority, StringComparison.OrdinalIgnoreCase))
            {
                path = uri2.AbsolutePath;
                return true;
            }
            return false;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool TryGetUrlPath(this HttpRequest request, string url, out string path)
        {
            path = null;
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri2) && uri2.Authority.Equals(request.Host.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                path = uri2.AbsolutePath;
                return true;
            }
            return false;
        }

    }
}
