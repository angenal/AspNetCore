using System.Text;

namespace NATS.Services.Config
{
    /// <summary>
    /// 连接redis
    /// </summary>
    public class RedisConfig
    {
        public string Addr { get; set; }
        public int Db { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }

        public override string ToString()
        {
            var s = new StringBuilder(Addr);
            if (Db > 0) s.AppendFormat(",defaultDatabase={0}", Db);
            if (!string.IsNullOrEmpty(Password)) s.AppendFormat(",password={0}", Password);
            if (!string.IsNullOrEmpty(Username)) s.AppendFormat(",user={0}", Username);
            return s.ToString();
        }
    }
}
