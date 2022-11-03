using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Hybrid.Caching
{
    public interface ICache
    {
        CacheType Type { get; }
        Task SetAsync<T>(string cacheKey, T cacheValue);
        Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration);
        Task<T> GetAsync<T>(string cacheKey);
        Task RemoveAsync(string cacheKey);
        Task<bool> ExistsAsync(string cacheKey);
        Task RemoveAllAsync(IEnumerable<string> cacheKeys);
        Task<T> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever);
        Task<T> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration);
    }
}
