using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using WebCore;

namespace NATS.Services.V8Script
{
    public static class ExtensionWeb
    {
        public static Tuple<string, string> MapPath(this Uri address) => new Tuple<string, string>(Environment.CurrentDirectory.Replace('\\', '/').TrimEnd('/') + '/', address.AbsoluteUri.Substring(0, 1 + address.AbsoluteUri.LastIndexOf('/')));

        public static Dictionary<string, string> ImageFileTypes = new Dictionary<string, string>
        {
            { "image/bmp", ".bmp" },
            { "image/gif", ".gif" },
            { "image/jpeg", ".jpg" },
            { "image/png", ".png" },
            { "image/pjpeg", ".jfif" },
            { "image/svg+xml", ".svg" },
            { "image/tiff", ".tiff" },
            { "image/x-icon", ".ico" },
            { "image/x-jg", ".art" },
        };

        public static Tuple<bool, string> DownloadFile(this Uri address, string dirName, string fileName = null, TimeSpan? timeout = null, bool checkFileExists = true, bool checkFileTypesExists = true)
        {
            if (dirName == null || !Directory.Exists(dirName)) return new Tuple<bool, string>(false, fileName);
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = (address.Authority + address.AbsolutePath).ToLower().Md5();
            }
            if (fileName.Length > 0 && checkFileExists)
            {
                if (File.Exists(Path.Combine(dirName, fileName))) return new Tuple<bool, string>(true, fileName);
                if (checkFileTypesExists)
                {
                    foreach (string extName in ImageFileTypes.Values)
                    {
                        if (File.Exists(Path.Combine(dirName, fileName + extName))) return new Tuple<bool, string>(true, fileName + extName);
                    }
                }
            }

            var ok = false;
            if (timeout == null || timeout == TimeSpan.Zero) timeout = TimeSpan.FromMinutes(2);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    string src = address.AbsolutePath;
                    int index = src.LastIndexOf('.'), cup = 5;
                    var hasExt = index > 0 && index > src.Length - cup;
                    string extName = hasExt ? src.Substring(index) : null;
                    index = fileName.LastIndexOf('.');
                    hasExt = index > 0 && index > fileName.Length - cup;
                    if (hasExt)
                    {
                        extName = fileName.Substring(index);
                        fileName = fileName.Substring(0, fileName.Length - extName.Length);
                    }

                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.82 Safari/537.36");

                        byte[] fileBytes = wc.DownloadData(address);
                        string fileType = wc.ResponseHeaders[HttpResponseHeader.ContentType];

                        if (string.IsNullOrEmpty(fileType) && string.IsNullOrEmpty(extName))
                            return;

                        if (fileType.ToLower().StartsWith("image"))
                        {
                            string tpy = fileType.ToLower();
                            foreach (string k in ImageFileTypes.Keys)
                            {
                                if (tpy != k) continue;
                                extName = ImageFileTypes[k];
                                break;
                            }
                        }

                        fileName += extName;
                        var path = Path.Combine(dirName, fileName);
                        File.WriteAllBytes(path, fileBytes);
                        ok = true;
                    }
                }
                catch (Exception) { }
            }).Wait(timeout.Value);
            return new Tuple<bool, string>(ok, fileName);
        }
    }
}
