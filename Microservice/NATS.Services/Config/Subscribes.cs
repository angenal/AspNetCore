namespace NATS.Services.Config
{
    /// <summary>
    /// 消息中间件/发布订阅记录列表
    /// </summary>
    public partial class Subscribes
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Spec { get; set; }
        public string Func { get; set; }
        public string Content { get; set; }
        public string CacheDir { get; set; }
        public int MsgLimit { get; set; }
        public int BytesLimit { get; set; }
        public int Amount { get; set; }
        public int Bulk { get; set; }
        public int Interval { get; set; }
        public long Version { get; set; }
        public long Created { get; set; }
        public bool Deleted { get; set; }
    }
}
