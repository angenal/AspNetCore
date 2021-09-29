using System;
using System.IO;

namespace WebFramework.Weixins.Data
{
    /// <summary>
    /// 微信小程序
    /// </summary>
    public static class WxOpen
    {
        /// <summary>
        /// 根目录
        /// </summary>
        public static string RootPath = Path.Combine(Environment.CurrentDirectory, "App_Data", "Weixin", nameof(WxOpen));

        /// <summary></summary>
        public static void Init(string file = "*.json")
        {
            if (!Directory.Exists(RootPath)) Directory.CreateDirectory(RootPath);
        }
    }
}
