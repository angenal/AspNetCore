using System;

namespace WebFramework.Weixins.DTO
{
    /// <summary>
    /// 图文素材
    /// </summary>
    [Serializable]
    public class NewsDTO
    {
        public string title { get; set; }
        public string author { get; set; }
        public string digest { get; set; }
        public string content { get; set; }
        public string content_source_url { get; set; }
        public string thumb_media_id { get; set; }
        public int show_cover_pic { get; set; }
        public string url { get; set; }
        public string thumb_url { get; set; }
        public int need_open_comment { get; set; }
        public int only_fans_can_comment { get; set; }
    }
}
