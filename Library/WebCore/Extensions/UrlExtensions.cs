using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="parameter"></param>
        /// <param name="timeout"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static string JsonRequest(this Uri uri, object parameter = null, int timeout = 10000, Dictionary<string, string> headers = null)
        {
            var s = parameter != null ? parameter.GetType().Name == "String" ? parameter.ToString() : parameter.ToJson() : "{}";
            return uri.Request(s, Encoding.UTF8, true, "application/json", headers, null, timeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="fieldName"></param>
        /// <param name="fileName"></param>
        /// <param name="responseText"></param>
        /// <param name="parameter"></param>
        /// <param name="timeout"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static bool UploadRequest(this Uri uri, string fieldName, string fileName, out string responseText, object parameter = null, int timeout = 120000, Dictionary<string, string> headers = null)
        {
            byte[] fileBytes;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, fileBytes.Length);
                fs.Close();
            }
            var client = new HttpPostRequestClient();
            client.SetFieldValue(fieldName, fileName, "application/octet-stream", fileBytes);
            if (parameter != null)
                foreach (var property in parameter.GetType().GetProperties())
                {
                    var s = property.GetValue(parameter)?.ToString();
                    if (string.IsNullOrEmpty(s))
                        continue;
                    client.SetFieldValue(property.Name, s);
                }
            return client.Upload(uri.ToString(), out responseText, timeout, headers);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="parameters"></param>
        /// <param name="encoding"></param>
        /// <param name="isPost"></param>
        /// <param name="contentType"></param>
        /// <param name="headers"></param>
        /// <param name="cookie"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static string Request(this Uri uri, string parameters, Encoding encoding, bool isPost = false, string contentType = "application/x-www-form-urlencoded", Dictionary<string, string> headers = null, CookieContainer cookie = null, int timeout = 120000)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Timeout = timeout;
            request.CookieContainer = cookie;
            if (headers != null) foreach (var item in headers) request.Headers.Add(item.Key, item.Value);
            if (isPost)
            {
                byte[] postData = encoding.GetBytes(parameters);
                request.Method = "POST";
                request.ContentType = contentType;
                request.ContentLength = postData.Length;
                using (Stream stream = request.GetRequestStream()) stream.Write(postData, 0, postData.Length);
            }
            string result;
            var response = (HttpWebResponse)request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                if (stream == null) return string.Empty;
                using (var reader = new StreamReader(stream, encoding)) result = reader.ReadToEnd();
            }
            return result;
        }

        internal class HttpPostRequestClient
        {
            private ArrayList bytesArray = new ArrayList();
            private readonly Encoding encoding = Encoding.UTF8;
            private readonly string boundary;

            public HttpPostRequestClient()
            {
                string flag = DateTime.Now.Ticks.ToString("x");
                boundary = "---------------------------" + flag;
            }

            private byte[] MergeContent()
            {
                int length = 0;
                int readLength = 0;
                string endBoundary = "--" + boundary + "--\r\n";
                byte[] endBoundaryBytes = encoding.GetBytes(endBoundary);
                bytesArray.Add(endBoundaryBytes);
                foreach (byte[] b in bytesArray)
                    length += b.Length;
                byte[] bytes = new byte[length];
                foreach (byte[] b in bytesArray)
                {
                    b.CopyTo(bytes, readLength);
                    readLength += b.Length;
                }
                return bytes;
            }

            public bool Upload(string requestUrl, out string responseText, int timeout = 120000, Dictionary<string, string> headers = null)
            {
                var webClient = new WebClient2(timeout);
                webClient.Headers.Add("Content-Type", "multipart/form-data; boundary=" + boundary);
                if (headers != null) foreach (var key in headers.Keys) webClient.Headers.Add(key, headers[key]);
                byte[] responseBytes;
                byte[] bytes = MergeContent();
                try
                {
                    responseBytes = webClient.UploadData(requestUrl, bytes);
                    responseText = Encoding.UTF8.GetString(responseBytes);
                    return true;
                }
                catch (WebException ex)
                {
                    var responseStream = ex.Response.GetResponseStream();
                    responseBytes = new byte[ex.Response.ContentLength];
                    responseStream.Read(responseBytes, 0, responseBytes.Length);
                }
                responseText = Encoding.UTF8.GetString(responseBytes);
                return false;
            }

            public void SetFieldValue(string fieldName, string fieldValue)
            {
                string httpRow = "--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}\r\n";
                string httpRowData = string.Format(httpRow, fieldName, fieldValue);
                bytesArray.Add(encoding.GetBytes(httpRowData));
            }

            public void SetFieldValue(string fieldName, string fileName, string contentType, byte[] fileBytes)
            {
                string end = "\r\n";
                string httpRow = "--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                string httpRowData = string.Format(httpRow, fieldName, fileName, contentType);
                byte[] endBytes = encoding.GetBytes(end);
                byte[] headerBytes = encoding.GetBytes(httpRowData);
                byte[] fileDataBytes = new byte[headerBytes.Length + fileBytes.Length + endBytes.Length];
                headerBytes.CopyTo(fileDataBytes, 0);
                fileBytes.CopyTo(fileDataBytes, headerBytes.Length);
                endBytes.CopyTo(fileDataBytes, headerBytes.Length + fileBytes.Length);
                bytesArray.Add(fileDataBytes);
            }
        }
        internal class WebClient2 : WebClient
        {
            private readonly int Timeout;
            public WebClient2(int timeout = 120000)
            {
                Timeout = timeout;
            }
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
                request.Timeout = Timeout;
                request.ReadWriteTimeout = Timeout;
                return request;
            }
        }
    }
}
