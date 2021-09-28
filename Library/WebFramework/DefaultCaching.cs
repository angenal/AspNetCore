using Microsoft.Extensions.Caching.Memory;

namespace WebFramework
{
    /// <summary>Default Cache</summary>
    public sealed class DefaultCaching
    {
        /// <summary>Memory Cache</summary>
        public static readonly MemoryCache Memory = new MemoryCache(new MemoryCacheOptions());
    }
}
