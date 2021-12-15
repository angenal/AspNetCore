using Microsoft.ClearScript;
using NATS.Client;
using System.Text;

namespace NATS.Services.V8Script
{
    /// <summary>
    /// 消息队列功能
    /// </summary>
    public sealed class JS_Nats
    {
        private IConnection Connection { get; set; }

        readonly string Prefix;
        readonly string Subject;

        public string prefix => Prefix;
        public string subject => Subject;

        public string name => Connection?.Opts.Name ?? null;

        public JS_Nats(IConnection connection, string prefix, string subject)
        {
            Connection = connection;
            Prefix = prefix;
            Subject = subject;
        }

        /// <summary>
        /// $nats.pub('data'); $nats.pub('subj','data')
        /// </summary>
        /// <param name="args"></param>
        public void pub(params object[] args)
        {
            var length = args.Length;
            if (length == 0)
                return;

            if (length == 1)
            {
                Connection?.Publish(Subject, Encoding.UTF8.GetBytes((args[0] as ScriptObject)?.ToJson() ?? args[0].ToString()));
                Connection?.Flush();
            }
            else if (length == 2)
            {
                Connection?.Publish(args[0].ToString(), Encoding.UTF8.GetBytes((args[1] as ScriptObject)?.ToJson() ?? args[1].ToString()));
                Connection?.Flush();
            }
        }

        /// <summary>
        /// $nats.req('data'); $nats.req('data',3); $nats.req('subj','data',3) // timeout:3s
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object req(params object[] args)
        {
            var length = args.Length;
            if (length == 0)
                return null;

            byte[] p = null;
            if (length == 1)
            {
                p = Connection?.Request(Subject, Encoding.UTF8.GetBytes((args[0] as ScriptObject)?.ToJson() ?? args[0].ToString())).Data;
                Connection?.Flush();
            }
            else if (length == 2)
            {
                p = int.TryParse(args[1].ToString(), out int timeout)
                    ? Connection?.Request(Subject, Encoding.UTF8.GetBytes((args[0] as ScriptObject)?.ToJson() ?? args[0].ToString()), timeout).Data
                    : Connection?.Request(args[0].ToString(), Encoding.UTF8.GetBytes((args[1] as ScriptObject)?.ToJson() ?? args[1].ToString())).Data;
                Connection?.Flush();
            }
            else if (length == 3)
            {
                p = int.TryParse(args[2].ToString(), out int timeout)
                    ? Connection?.Request(args[0].ToString(), Encoding.UTF8.GetBytes((args[1] as ScriptObject)?.ToJson() ?? args[1].ToString()), timeout).Data
                    : Connection?.Request(args[0].ToString(), Encoding.UTF8.GetBytes((args[1] as ScriptObject)?.ToJson() ?? args[1].ToString())).Data;
                Connection?.Flush();
            }

            return p == null && p.Length == 0 ? null : Encoding.UTF8.GetString(p);
        }
    }
}
