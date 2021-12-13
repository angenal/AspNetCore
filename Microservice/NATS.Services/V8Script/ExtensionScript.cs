using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NATS.Services.V8Script
{
    public static class ExtensionScript
    {
        /// <summary>
        /// DateTime.Now.js() => new Date()
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static object js(this DateTime dt)
        {
            var t = dt.Add(TimeZoneInfo.Local.GetUtcOffset(dt));
            object createDate = ScriptEngine.Current.Script.__createDate;
            if (createDate is Undefined)
                createDate = ScriptEngine.Current.Evaluate(@"__createDate=function(yr,mo,day,hr,min,sec,ms){return new Date(yr,mo,day,hr,min,sec,ms)}");
            //createDate = ScriptEngine.Current.Evaluate(@"__createDate=function(yr,mo,day,hr,min,sec,ms){return new Date(Date.UTC(yr,mo,day,hr,min,sec,ms)).toLocaleString('zh-CN',{timeZone:'Asia/Shanghai'})}");
            return ((dynamic)createDate)(t.Year, t.Month - 1, t.Day, t.Hour, t.Minute, t.Second, t.Millisecond);
        }

        #region Html Document Parser

        /// <summary>
        /// query HtmlNode[] selectors
        /// https://www.w3.org/TR/selectors-3/
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<HtmlNode> query(this HtmlDocument doc, string selector, string separator = "\n")
        {
            var nodes = new List<HtmlNode>();
            foreach (string i in selector.Split(separator.ToCharArray()).Where(i => !string.IsNullOrWhiteSpace(i)))
                nodes.AddRange(doc.DocumentNode.QuerySelectorAll(i));
            return nodes;
        }

        /// <summary>
        /// query HtmlNode[] selectors is List String ScriptObject
        /// https://www.w3.org/TR/selectors-3/
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<HtmlNode> query(this HtmlDocument doc, ScriptObject selector)
        {
            var nodes = new List<HtmlNode>();
            foreach (string i in selector.ToListString())
                nodes.AddRange(doc.DocumentNode.QuerySelectorAll(i));
            return nodes;
        }

        /// <summary>
        /// query HtmlNode[] selectors is params string[]
        /// https://www.w3.org/TR/selectors-3/
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<HtmlNode> query(this HtmlDocument doc, params string[] selector)
        {
            var nodes = new List<HtmlNode>();
            foreach (string i in selector.Where(i => !string.IsNullOrWhiteSpace(i)))
                nodes.AddRange(doc.DocumentNode.QuerySelectorAll(i));
            return nodes;
        }

        /// <summary>
        /// query Html selectors
        /// https://www.w3.org/TR/selectors-3/
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static string html(this HtmlDocument doc, string selector, bool innerHtml = false) => html(query(doc, selector), innerHtml);

        /// <summary>
        /// query Html selectors is List String ScriptObject
        /// https://www.w3.org/TR/selectors-3/
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="selector"></param>
        /// <param name="innerHtml"></param>
        /// <returns></returns>
        public static string html(this HtmlDocument doc, ScriptObject selector, bool innerHtml = false) => html(query(doc, selector), innerHtml);

        /// <summary>
        /// query Html selectors is params string[]
        /// https://www.w3.org/TR/selectors-3/
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static string html(this HtmlDocument doc, params string[] selector) => html(query(doc, selector), false);

        /// <summary>
        /// query object[] selectors
        /// https://www.w3.org/TR/selectors-3/
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static object[] json(this HtmlDocument doc, string selector) => json(query(doc, selector));

        /// <summary>
        /// query object[] selectors is List String ScriptObject
        /// https://www.w3.org/TR/selectors-3/
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static object[] json(this HtmlDocument doc, ScriptObject selector) => json(query(doc, selector));

        /// <summary>
        /// query object[] selectors is params string[]
        /// https://www.w3.org/TR/selectors-3/
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static object[] json(this HtmlDocument doc, params string[] selector) => json(query(doc, selector));

        /// <summary>
        /// json object[] nodes.json()
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static object[] json(this IEnumerable<HtmlNode> nodes)
        {
            var items = new object[nodes.Count()];
            for (var i = 0; i < items.Length; i++) items[i] = json(nodes.ElementAt(i));
            return items;
        }

        /// <summary>
        /// json object node.json()
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static object json(this HtmlNode node)
        {
            dynamic item = new System.Dynamic.ExpandoObject();
            item.html = node.InnerHtml?.Trim();
            item.text = node.InnerText?.Trim();
            item.tagName = node.Name;
            foreach (HtmlAttribute attr in node.Attributes)
            {
                switch (attr.Name)
                {
                    case "id": item.id = attr.Value; break;
                    case "alt": item.alt = attr.Value; break;
                    case "class": item.className = attr.Value; break;
                    case "content": item.content = attr.Value; break;
                    case "name": item.name = attr.Value; break;
                    case "href": item.href = attr.Value; break;
                    case "src": item.src = attr.Value; break;
                    case "style": item.style = attr.Value; break;
                    case "target": item.target = attr.Value; break;
                    case "type": item.type = attr.Value; break;
                    case "title": item.title = attr.Value; break;
                    case "value": item.value = attr.Value; break;
                    default: break;
                }
            }
            return item;
        }

        /// <summary>
        /// html string nodes.html()
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string html(this IEnumerable<HtmlNode> nodes, bool innerHtml = false)
        {
            var s = new StringBuilder();
            foreach (HtmlNode node in nodes) s.Append(html(node, innerHtml));
            return s.ToString();
        }

        /// <summary>
        /// html string node.html()
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string html(this HtmlNode node, bool innerHtml = false)
        {
            if (innerHtml) return node.InnerHtml?.Trim();

            var s = new StringBuilder();
            s.AppendFormat("<{0}", node.Name);
            foreach (HtmlAttribute attr in node.Attributes) s.AppendFormat(" {0}=\"{1}\"", attr.Name, attr.Value?.Replace("\"", "'"));
            s.Append(">");
            s.Append(node.InnerHtml?.Trim());
            s.AppendFormat("</{0}>", node.Name);
            return s.ToString();
        }

        /// <summary>
        /// doc.save(path)
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="path"></param>
        public static void save(this HtmlDocument doc, string path) => File.WriteAllText(path, doc.DocumentNode.OuterHtml);

        #endregion

        /// <summary>
        /// Init Add Host Extensions
        /// </summary>
        /// <param name="engine"></param>
        public static void AddHostExtensions(this V8ScriptEngine engine)
        {
            #region function setTimeout(func, delay)

            engine.Script._setTimeout = new Action<ScriptObject, int>((func, delay) =>
            {
                var timer = new Timer(_ => func.Invoke(false));
                timer.Change(delay, Timeout.Infinite);
            });

            engine.Execute(@"
function setTimeout(func, delay) {
    let args = Array.prototype.slice.call(arguments, 2);
    _setTimeout(func.bind(undefined, ...args), delay || 0);
}
            ");

            #endregion

            #region function $.loadxml(text) return XmlDocument

            //            engine.Execute(@"
            //$.loadxml = function (text) {
            //    var doc = new XmlDocument();
            //    doc.LoadXml(text);
            //    return doc;
            //};
            //            ");

            #endregion

            #region function $.loadhtml(text), $.loadurl(url) return HtmlDocument

            //            engine.Execute(@"
            //$.loadhtml = function (text) {
            //    var doc = new HtmlDocument();
            //    if (!text) return doc;
            //    let c = text[0]; if (c == 'A' || c == 'B' || c == 'C' || c == 'D' || c == 'E' || c == 'F') text = $.loadfile(text);
            //    doc.LoadHtml(text);
            //    return doc;
            //};
            //            ");

            //            engine.Execute(@"
            //$.loadurl = function (url) {
            //    let duration = arguments.length > 1 ? arguments[1] : $.timeout();
            //    let userAgent = arguments.length > 2 ? arguments[2] : 'Mozilla /5.0 (Macintosh; Intel Mac OS X 10_14_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.105 Safari/537.36';
            //    let web = new HtmlWeb();
            //    web.AutoDetectEncoding = true;
            //    web.UsingCache = false;
            //    web.BrowserTimeout = TimeSpan.FromSeconds(duration);
            //    web.UserAgent = userAgent;
            //    var doc = web.Load(url);
            //    return doc;
            //};
            //            ");

            #endregion
        }
    }
}
