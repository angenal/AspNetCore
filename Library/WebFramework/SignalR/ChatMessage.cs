using System;
using System.Collections.Generic;
using System.Linq;

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
        public DateTime Time { get; set; }
    }

    /// <summary>
    /// 聊天消息管理
    /// </summary>
    public class ChatMessage
    {
        /// <summary></summary>
        public static void Add(string groupName, Message message) => Data.RedisList.Add(groupName, message);
        /// <summary></summary>
        public static IEnumerable<Message> Get(string groupName, int size = 20) => Data.RedisList.GetLastestResult<Message>(groupName, size).OrderByDescending(t => t.Time);
    }
}
