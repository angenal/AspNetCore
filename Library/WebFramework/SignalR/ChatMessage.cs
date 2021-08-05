using System;

namespace WebFramework.SignalR
{
    /// <summary>
    /// 聊天消息
    /// </summary>
    public class Message
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 消息来源
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
