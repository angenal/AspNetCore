using System;

namespace WebFramework.Weixins.DTO
{
    /// <summary>
    /// 图片
    /// </summary>
    [Serializable]
    public class ImageDTO
    {
        public string media_id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }
}
