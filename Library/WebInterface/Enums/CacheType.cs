using System.ComponentModel;

namespace WebInterface
{
    /// <summary>
    /// 缓存类型: 0.内存缓存 1.Redis缓存 2.内存+Redis缓存
    /// </summary>
    public enum CacheType
    {
        /// <summary>
        /// 内存缓存
        /// </summary>
        [Description("内存缓存")]
        Memory,
        /// <summary>
        /// Redis缓存
        /// </summary>
        [Description("Redis缓存")]
        Redis,
        /// <summary>
        /// 内存+Redis缓存
        /// </summary>
        [Description("内存+Redis缓存")]
        All,
    }
}
