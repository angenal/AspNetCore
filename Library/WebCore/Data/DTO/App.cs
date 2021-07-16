using System.Collections.Generic;

namespace WebCore.Data.DTO
{
    /// <summary>
    /// 基本的应用列表信息
    /// </summary>
    public static class App
    {
        /// <summary>
        /// 应用列表信息
        /// </summary>
        public static List<IAppInfo> Infos;
    }

    /// <summary>
    /// 基本的应用信息
    /// </summary>
    public class AppInfo: IAppInfo
    {
        /// <summary>
        /// 应用标识
        /// </summary>
        public string appid { get; set; }
        /// <summary>
        /// 应用名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 凭证密钥
        /// </summary>
        public string secret { get; set; }
    }
    public interface IAppInfo
    {
        /// <summary>
        /// 应用标识
        /// </summary>
        string appid { get; set; }
        /// <summary>
        /// 凭证密钥
        /// </summary>
        string secret { get; set; }
    }
}
