using System;

namespace WebFramework.Weixins.DTO
{
    /// <summary>
    /// 消息接收与回复
    /// </summary>
    [Serializable]
    public class RequestReplyDTO
    {
        /// <summary>
        /// 输入keyword
        /// </summary>
        public string keyword { get; set; }
        /// <summary>
        /// 回复：图片（image）、图文（news）
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 缓存 media
        /// </summary>
        public string media_id { get; set; }
    }
}
