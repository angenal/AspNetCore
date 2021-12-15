using CommandLine;
using System;

namespace NATS.Services
{
    /// <summary>
    /// 命令行参数解析
    /// </summary>
    class Flags
    {
        [Option('c', "config", Required = true, Default = "natsql.yaml", HelpText = "sets config file")]
        public string Config { get; set; }

        [Option('t', "test", HelpText = "sets json file and run SQL test")]
        public string Test { get; set; }

        [Option('i', "interval", Default = 0, HelpText = "sets auto create subscribes interval seconds")]
        public int CreateInterval { get; set; }

        [Option('a', "address", HelpText = "the NatS-Server address")]
        public string Addr { get; set; }

        [Option('n', "name", HelpText = "the NatS-Subscription name prefix")]
        public string Name { get; set; }

        [Option('k', "token", Required = true, HelpText = "the NatS-Token auth string")]
        public string Token { get; set; }

        [Option('f', "cred", HelpText = "the NatS-Cred file")]
        public string Cred { get; set; }

        [Option('l', "cert", HelpText = "the NatS-TLS cert file")]
        public string Cert { get; set; }

        [Option('s', "key", HelpText = "the NatS-TLS key file")]
        public string Key { get; set; }

        public static void Usage()
        {
            Console.WriteLine(@" Usage of natsql:
  -c --config string
        sets config file (default ""natsql.yaml"")
  -t --test string
        sets json file and run SQL test
  -i --interval int
        sets auto create subscribes interval seconds
  -a --address string
        the NatS - Server address
  -n --name string
        the NatS - Subscription name prefix
  -k --token string
        the NatS - Token auth string
  -f --cred string
        the NatS - Cred file
  -l --cert string
        the NatS - TLS cert file
  -s --key string
        the NatS - TLS key file");
        }
    }
}
