namespace NATS.Services
{
    /// <summary>
    /// 消息中间件/发布订阅处理
    /// </summary>
    public class NatsConfig
    {
        public string Name { get; set; }
        public string Addr { get; set; }
        public string Token { get; set; }
        public string Cred { get; set; }
        public string Cert { get; set; }
        public string Key { get; set; }
        public string Subscribe { get; set; }
        public string CacheDir { get; set; }
        public int MsgLimit { get; set; }
        public int BytesLimit { get; set; }
        public int Amount { get; set; }
        public int Bulk { get; set; }
        public int Interval { get; set; }
        public string Script { get; set; }
        public string Spec { get; set; }
        public string Table { get; set; }
        public long Created { get; set; }
        public long Version { get; set; }
        public bool Deleted { get; set; }
    }
}
