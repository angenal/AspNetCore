using EasyCaching.Core;
using EasyCaching.LiteDB;
using LiteDB;

namespace WebFramework
{
    public static class EasyCachingDataSource
    {
        public const string Memory = "DefaultInMemory";
        public const string Redis = "DefaultRedis";
        public const string LiteDB = "DefaultLiteDB";

        public static IEasyCachingProvider MemoryProvider(this IEasyCachingProviderFactory factory) => factory.GetCachingProvider(Memory);
        public static IEasyCachingProvider RedisProvider(this IEasyCachingProviderFactory factory) => factory.GetCachingProvider(Redis);
        public static IEasyCachingProvider LiteDBProvider(this IEasyCachingProviderFactory factory) => factory.GetCachingProvider(LiteDB);

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
