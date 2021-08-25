using EasyCaching.Core;
using EasyCaching.LiteDB;
using LiteDB;

namespace WebFramework
{
    /// <summary>
    /// EasyCaching  https://easycaching.readthedocs.io
    /// </summary>
    public static class EasyCachingDataSource
    {
        /// <summary></summary>
        public static IEasyCachingProvider MemoryProvider(this IEasyCachingProviderFactory factory) => factory.GetCachingProvider(EasyCachingConstValue.DefaultInMemoryName);
        /// <summary></summary>
        public static IEasyCachingProvider RedisProvider(this IEasyCachingProviderFactory factory) => factory.GetCachingProvider(EasyCachingConstValue.DefaultRedisName);
        /// <summary></summary>
        public static IEasyCachingProvider LiteDBProvider(this IEasyCachingProviderFactory factory) => factory.GetCachingProvider(EasyCachingConstValue.DefaultLiteDBName);

        /// <summary></summary>
        public static LiteDBDBOptions GetLiteDBDBOptions(string connectionString)
        {
            var c = new ConnectionString(connectionString);
            return new LiteDBDBOptions()
            {
                FilePath = c.Filename,
                Password = c.Password,
                InitialSize = c.InitialSize,
                ConnectionType = c.Connection
            };
        }
    }
}
