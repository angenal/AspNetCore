using System;
using System.Threading.Tasks;

namespace WebCore.Data
{
    /// <summary>The interface for a cache manager with supports asynchronous, task based item creation functions.</summary>
    public interface IAsyncCacheManager<TKey, TItem>
    {
        /// <summary>Gets an existing item or asynchronously creates a new one.</summary>
        /// <param name="key">The key of the item.</param>
        /// <param name="creationFunction">The item creator.</param>
        /// <returns>The item.</returns>
        Task<TItem> GetOrCreateAsync(TKey key, Func<Task<TItem>> creationFunction);
    }
}
