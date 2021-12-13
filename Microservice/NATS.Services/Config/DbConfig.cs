namespace NATS.Services.Config
{
    /// <summary>
    /// 数据库连接client
    /// </summary>
    public class DbConfig
    {
        /// <summary>
        /// 支持mssql,mysql
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string Conn { get; set; }
        /// <summary>
        /// enable debug
        /// </summary>
        public bool Debug { get; set; }
    }
}
