using HtmlAgilityPack;
using Microsoft.ClearScript;
using NATS.Services.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NATS.Services.V8Script
{
    public static class ConsoleJs
    {
        /// <summary>
        /// console.log(args)
        /// console.log('{0:G}',new Date)
        /// </summary>
        /// <param name="args"></param>
        public static void log(params object[] args)
        {
            Console.Write("console.log > ");
            foreach (var arg in args)
            {
                if (arg == null || arg == DBNull.Value)
                {
                    Console.Write("null ");
                    continue;
                }

                if (arg is Undefined)
                {
                    Console.Write("undefined ");
                    continue;
                }

                if (arg is string && Regex.IsMatch(arg.ToString(), @"\{\d+:?\w*\}"))
                {
                    Console.Write(arg.ToString(), args.Skip(1).ToArray());
                    Console.Write(" ");
                    break;
                }

                if (arg is string || arg is char || arg is ulong || arg is long || arg is int || arg is uint || arg is bool || arg is decimal || arg is float || arg is double)
                {
                    Console.Write(arg);
                    Console.Write(" ");
                    continue;
                }

                if (arg is DateTime)
                {
                    Console.Write(((DateTime)arg).ToString(NewtonsoftJson.DateTimeFormat));
                    Console.Write(" ");
                    continue;
                }

                #region HtmlNode

                if (arg is HtmlNode node0)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(node0.html());
                    continue;
                }

                if (arg is IEnumerable<HtmlNode> nodes1)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    foreach (HtmlNode node1 in nodes1) Console.WriteLine(node1.html());
                    continue;
                }

                if (arg is HtmlNodeCollection nodes2)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    foreach (HtmlNode node1 in nodes2) Console.WriteLine(node1.html());
                    continue;
                }

                #endregion

                var s = JsonConvert.SerializeObject(arg, NewtonsoftJson.Converters);
                Console.Write(s);
                Console.Write(" ");
            }
            Console.WriteLine();
        }
    }
}
