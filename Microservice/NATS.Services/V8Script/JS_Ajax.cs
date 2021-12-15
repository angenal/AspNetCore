using HtmlAgilityPack;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using NATS.Services.Util;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using WebCore;

namespace NATS.Services.V8Script
{
    /// <summary>
    /// Ajax功能
    /// </summary>
    public sealed class JS_Ajax
    {
        readonly Dictionary<string, object> Values = new Dictionary<string, object>();
        readonly V8ScriptEngine Engine;
        readonly RestClient client;

        public JS_Ajax(V8ScriptEngine engine)
        {
            Engine = engine;
            client = new RestClient();
            client.AddDefaultHeader("Accept", "*/*");
            client.AddDefaultHeader("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
            client.UserAgent = "Mozilla /5.0 (Macintosh; Intel Mac OS X 10_14_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.105 Safari/537.36";
            client.FailOnDeserializationError = false;
            client.ThrowOnDeserializationError = false;
            client.FollowRedirects = true;
            client.MaxRedirects = 2;
            client.ReadWriteTimeout = 60 * 1000;
            client.Timeout = 60 * 1000; // default timeout 60s
            //client.ThrowOnAnyError = true;
            // Override with Custom Newtonsoft JSON Handler
            //client.AddJsonHandler();
        }

        /// <summary>
        /// var uuid = $.uuid()
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object uuid(params object[] args)
        {
            if (args.Length > 0)
                return Guid.NewGuid().ToString(args[0].ToString());
            return Guid.NewGuid().ToString().ToLower();
        }

        /// <summary>
        /// var guid = $.guid("N")
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object guid(params object[] args)
        {
            if (args.Length > 0)
                return Guid.NewGuid().ToString(args[0].ToString());
            return Guid.NewGuid().ToString().ToLower();
        }

        /// <summary>
        /// var i = $.crc("text")
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public object crc(params object[] args) => args.Length == 0 ? 0 : string.Join("", args).Crc32();

        /// <summary>
        /// var i = $.md5("text")
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public object md5(params object[] args) => args.Length == 0 ? "" : string.Join("", args).Md5();

        /// <summary>
        /// var ok = $.v("n",1)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object v(params object[] args)
        {
            if (args.Length == 0) return false;

            string k = args[0].ToString();
            if (string.IsNullOrEmpty(k)) return false;

            if (args.Length == 1)
            {
                return Values.Keys.Contains(k) ? Values[k] : false;
            }

            if (args.Length == 2)
            {
                Values[k] = args[1];
                return true;
            }

            Values[k] = args.Skip(1).ToArray();
            return true;
        }

        /// <summary>
        /// var res = $.q("get",url)
        /// var res = $.q("get",url,param)
        /// var res = $.q("post",url,param,"json")
        /// </summary>
        /// <param name="args"></param>
        public object q(params object[] args) => args.Length < 2 ? null : Request(args[0].ToString(), args.Skip(1).ToArray());

        /// <summary>
        /// var res = $.get(url)
        /// var res = $.get(url,param)
        /// </summary>
        /// <param name="args"></param>
        public object get(params object[] args) => Request("get", args);

        /// <summary>
        /// var res = $.post(url,param,"json")
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object post(params object[] args) => Request("post", args);

        /// <summary>
        /// var seconds = $.timeout()
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object timeout(params object[] args)
        {
            if (args.Length == 1)
            {
                if (int.TryParse(args[0].ToString(), out int i) && i > 0)
                    client.Timeout = client.ReadWriteTimeout = i * 1000;
            }
            if (args.Length >= 2)
            {
                if (int.TryParse(args[0].ToString(), out int i) && i > 0)
                    client.Timeout = i * 1000;
                if (int.TryParse(args[1].ToString(), out int j) && j > 0)
                    client.ReadWriteTimeout = j * 1000;
            }
            return client.Timeout / 1000;
        }

        Tuple<Method, Uri, RestRequest> GetRequestMethodAndUrl(object[] args)
        {
            string methodArg = args[0].ToString(), url = args[1].ToString();
            if (methodArg.StartsWith("http")) { var str = methodArg; methodArg = url; url = str; }
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri resource))
                return null;

            var baseUrl = new Uri($"{resource.Scheme}://{resource.Authority}/");
            var method = Enum.TryParse(methodArg.ToUpper(), out Method method1) ? method1 : Method.GET;

            var request = new RestRequest(resource.PathAndQuery.TrimStart('/'));
            request.RequestFormat = DataFormat.None;
            request.ReadWriteTimeout = client.ReadWriteTimeout;
            request.Timeout = client.Timeout;

            return new Tuple<Method, Uri, RestRequest>(method, baseUrl, request);
        }

        public object Request(string method, params object[] args)
        {
            var length = args.Length;
            if (length == 0)
                return null;

            var param = GetRequestMethodAndUrl(new object[] { method }.Concat(args).ToArray());
            if (param == null)
                return null;

            client.BaseUrl = param.Item2;
            var request = param.Item3;

            var jsonEncoding = length > 2 && args[2].ToString() == "json";
            if (jsonEncoding) request.RequestFormat = DataFormat.Json;
            var xmlEncoding = length > 2 && args[2].ToString() == "xml";
            if (xmlEncoding) request.RequestFormat = DataFormat.Xml;

            if (length > 1 && args[1] != null)
            {
                var obj = args[1] as ScriptObject;

                if (jsonEncoding)
                {
                    request.AddHeader("Content-Type", "application/json;charset=utf-8");
                    if (obj != null) request.AddCustomJsonBody(obj.ToDictionary());
                    else request.AddJsonBody(args[1]);
                }
                else if (xmlEncoding)
                {
                    request.AddParameter("", args[1].ToString(), "text/xml", ParameterType.RequestBody);
                }
                else
                {
                    if (param.Item1 == Method.GET)
                    {
                        if (obj != null) foreach (var key in obj.PropertyNames) request.AddQueryParameter(key, Convert.ToString(obj.GetProperty(key)), true);
                        else request.AddQueryParameter("", args[1].ToString(), false);
                    }
                    else if (param.Item1 == Method.POST || param.Item1 == Method.PUT || param.Item1 == Method.PATCH || param.Item1 == Method.DELETE)
                    {
                        if (obj != null) foreach (var key in obj.PropertyNames) request.AddParameter(key, obj.GetProperty(key), "application/x-www-form-urlencoded", ParameterType.GetOrPost);
                        else request.AddParameter("", args[1].ToString(), "application/x-www-form-urlencoded", ParameterType.GetOrPost);
                    }
                    else
                    {
                        if (obj != null) request.AddCustomJsonBody(obj.ToDictionary());
                        else request.AddJsonBody(args[1]);
                    }
                }
            }

            var res = client.Execute(request, param.Item1);
            if (res.ResponseStatus != ResponseStatus.Completed)
                return new { code = -1, error = res.ErrorException.Message };
            if (res.StatusCode != System.Net.HttpStatusCode.OK)
                return new { code = res.StatusCode.GetHashCode(), error = res.StatusDescription };

            if (string.IsNullOrWhiteSpace(res.Content))
                return null;

            var content = res.Content.Trim();

            jsonEncoding = content.StartsWith("{") && content.EndsWith("}");

            return jsonEncoding ? Engine.Evaluate(JS.SecurityCode(content)) : content;
        }

        /// <summary>
        /// var fileName = $.download('https://www.baidu.com/favicon.ico')
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object download(params object[] args)
        {
            var length = args.Length;
            if (length == 0 || !Uri.TryCreate(args[0].ToString(), UriKind.Absolute, out Uri address))
                return null;

            string dir = "", name = "";
            TimeSpan timeout = TimeSpan.FromMilliseconds(client.Timeout);

            if (length > 1 && args[1] != null)
                parseArgs(args[1].ToString(), ref dir, ref name, ref timeout);
            if (length > 2 && args[2] != null)
                parseArgs(args[2].ToString(), ref dir, ref name, ref timeout);
            if (length > 3 && args[3] != null)
                parseArgs(args[3].ToString(), ref dir, ref name, ref timeout);

            if (string.IsNullOrEmpty(dir)) dir = tempdir().ToString();
            if (string.IsNullOrEmpty(name))
            {
                var arg1 = address.AbsolutePath;
                int index = arg1.LastIndexOf('.'), cup = 5;
                var hasExt = index > arg1.Length - cup;
                if (hasExt) name = arg1.Substring(1 + arg1.LastIndexOf('/'));
            }

            var file = address.DownloadFile(dir, name, timeout, name != null && name.Length > 0 && !name.EndsWith(".tmp"));
            if (!file.Item1)
                return null;

            return Path.Combine(dir, file.Item2);

            void parseArgs(string arg1, ref string dir1, ref string name1, ref TimeSpan timeout1)
            {
                if (arg1.Contains(Path.DirectorySeparatorChar) || arg1.Contains('/'))
                {
                    dir1 = arg1;
                    int index = arg1.LastIndexOf('.'), cup = 5;
                    var hasExt = index > arg1.Length - cup;
                    if (hasExt)
                    {
                        index = arg1.Contains('/') ? arg1.LastIndexOf('/') : arg1.LastIndexOf(Path.DirectorySeparatorChar);
                        dir1 = arg1.Substring(0, index);
                        name1 = arg1.Substring(index + 1);
                    }
                }
                else if (arg1.Contains('.'))
                {
                    name1 = arg1;
                }
                else if (int.TryParse(arg1, out int s))
                {
                    timeout1 = TimeSpan.FromSeconds(s);
                }
                else
                {
                    name1 = arg1;
                }
            }
        }

        /// <summary>
        /// var tempdir = $.tempdir()
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object tempdir(params object[] args)
        {
            var dir = Environment.GetEnvironmentVariable("TEMP");
            if (string.IsNullOrEmpty(dir)) dir = Environment.GetEnvironmentVariable("TMP");
            if (string.IsNullOrEmpty(dir)) dir = Path.GetTempPath();
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>
        /// var tmpfile = $.tempfile(), randomfile = $.tempfile("random")
        /// var tmpfile = $.tempfile('https://www.baidu.com/', '.html')
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object tempfile(params object[] args)
        {
            var extension = Path.GetExtension(args.Length > 1 ? "0." + args[1].ToString() : "0.tmp");
            if (args.Length > 0 && Uri.TryCreate(args[0].ToString(), UriKind.Absolute, out Uri address))
                return Path.Combine(tempdir().ToString(), (address.Authority + address.AbsolutePath).ToLower().Md5() + extension);
            return Path.ChangeExtension(args.Length > 0 && "random" == args[0].ToString() ? Path.Combine(tempdir().ToString(), Path.GetRandomFileName()) : Path.GetTempFileName(), extension);
        }

        /// <summary>
        /// var fs = $.loaddir(path)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object loaddir(params object[] args)
        {
            if (args.Length == 0)
                return null;

            var path = args[0].ToString();
            var s = new List<string>();
            s.AddRange(Directory.GetFiles(path));
            s.AddRange(Directory.GetDirectories(path));
            return Engine.Evaluate(JsonConvert.SerializeObject(s));
        }

        /// <summary>
        /// var text = $.loadfile(path)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object loadfile(params object[] args)
        {
            if (args.Length == 0)
                return null;

            var path = args[0].ToString();
            if (!File.Exists(path))
                return null;

            return File.ReadAllText(path);
        }

        /// <summary>
        /// var ok = $.existsdir(path)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object existsdir(params object[] args)
        {
            if (args.Length == 0)
                return null;

            var path = args[0].ToString();
            return Directory.Exists(path);
        }

        /// <summary>
        /// var ok = $.existsfile(path)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object existsfile(params object[] args)
        {
            if (args.Length == 0)
                return null;

            var path = args[0].ToString();
            return File.Exists(path);
        }

        /// <summary>
        /// var ok = $.deletefile(path)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object deletefile(params object[] args)
        {
            if (args.Length == 0)
                return null;

            var path = args[0].ToString();
            File.Delete(path);
            return !File.Exists(path);
        }

        /// <summary>
        /// var doc = $.loadxml(path)
        /// var doc = $.loadxml(text)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object loadxml(params object[] args)
        {
            if (args.Length == 0)
                return null;

            var doc = new XmlDocument();

            var text = args[0].ToString();
            if (!text.TrimStart().StartsWith("<") && Uri.IsWellFormedUriString(text, UriKind.RelativeOrAbsolute) || File.Exists(text))
                doc.Load(text);
            else
                doc.LoadXml(text);
            return doc;
        }

        /// <summary>
        /// var doc = $.loadhtml(path)
        /// var doc = $.loadhtml(text)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object loadhtml(params object[] args)
        {
            if (args.Length == 0)
                return null;

            var doc = new HtmlDocument();

            var text = args[0].ToString();
            if (!text.TrimStart().StartsWith("<") && Uri.IsWellFormedUriString(text, UriKind.RelativeOrAbsolute) || File.Exists(text))
                doc.Load(text, true);
            else
                doc.LoadHtml(text);
            return doc;
        }

        /// <summary>
        /// var doc = $.loadurl(url)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object loadurl(params object[] args)
        {
            if (args.Length == 0)
                return null;

            var url = args[0].ToString();
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri _))
                return null;

            var duration = args.Length > 1 && args[1].GetType().Name.StartsWith("Int") ? int.Parse(args[1].ToString()) : client.Timeout / 1000;
            var userAgent = args.Length > 2 ? args[2].ToString() : client.UserAgent;

            var web = new HtmlWeb
            {
                AutoDetectEncoding = true,
                CaptureRedirect = true,
                UsingCacheIfExists = true,
                CachePath = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache),
                UserAgent = userAgent
            };
            var doc = duration > 1 && duration <= 60
                ? web.LoadFromWebAsync(url, new CancellationTokenSource(TimeSpan.FromSeconds(duration)).Token).Result
                : web.Load(url);
            return doc;
        }

        /// <summary>
        /// 压缩图片文件(文件路径,命令行参数,有损压缩模式=true,覆盖文件=true)
        /// var res = $.compressimage('D:/Temp/01.png', 'pingo -s9 -pngpalette=50 -strip -q')
        /// var res = $.compressimage('D:/Temp/02.jpg', 'cjpeg -quality 60,40 -optimize -dct float -smooth 5')
        /// var res = $.compressimage('D:/Temp/03.gif', 'gifsicle -O3')
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object compressimage(params object[] args)
        {
            var length = args.Length;
            if (length == 0)
                return null;

            string fileName = args[0].ToString(), cmdArguments = length > 1 && "String" == args[1].GetType().Name ? args[1].ToString() : null;
            if (fileName.Contains(" -") && !cmdArguments.Contains(" -"))
            {
                string temp = fileName; fileName = cmdArguments; cmdArguments = temp;
            }

            bool lossy = length > 2 && "Boolean" == args[2].GetType().Name ? (bool)args[2] : true;
            bool overwrite = length > 3 && "Boolean" == args[3].GetType().Name ? (bool)args[3] : true;

            var result = ImageCompress.CompressFile(fileName, cmdArguments, lossy, overwrite);
            return result != null ? Engine.Evaluate(JS.SecurityCode(JsonConvert.SerializeObject(result))) : null;
        }
    }
}
