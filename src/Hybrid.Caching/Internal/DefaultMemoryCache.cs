using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hybrid.Caching.Configurations;

namespace Hybrid.Caching.Internal
{
    internal class DefaultMemoryCache : ICache, IMetriable
    {
        private readonly IMemoryCache _cache;
        private readonly CachingOptions _options;
        private readonly Metrics _metrics;

        public DefaultMemoryCache(CachingOptions options, IMemoryCache memoryCache, ILogger<DefaultMemoryCache> logger)
        {
            _cache = memoryCache;
            _options = options;
            _metrics = new Metrics(logger);
        }

        public CacheType Type { get; } = CacheType.Memory;

        public Task<bool> ExistsAsync(string cacheKey)
        {
            var exists = _cache.TryGetValue(cacheKey, out var _);

            return Task.FromResult(exists);
        }

        public Task<T> GetAsync<T>(string cacheKey)
        {
            var result = _cache.Get<T>(cacheKey);

            return Task.FromResult(result);
        }

        public Task<T> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever)
        {
            return GetAsync<T>(cacheKey, dataRetriever, _options.DefaultExpiration);
        }

        public async Task<T> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            var onMiss = false;

            var @value = await _cache.GetOrCreateAsync<T>(cacheKey, (entry) =>
            {
                onMiss = true;

                entry.AbsoluteExpirationRelativeToNow = expiration;

                return dataRetriever();
            }).ConfigureAwait(false);

            if (onMiss)
                _metrics.OnMiss(cacheKey);
            else
                _metrics.OnHit(cacheKey);

            return @value;
        }

        public Metrics GetMetrics() => _metrics;

        public Task RemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            foreach (var cacheKey in cacheKeys)
                _cache.Remove(cacheKey);

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string cacheKey)
        {
            _cache.Remove(cacheKey);

            return Task.CompletedTask;
        }

        public Task SetAsync<T>(string cacheKey, T cacheValue)
        {
            return SetAsync<T>(cacheKey, cacheValue, _options.DefaultExpiration);
        }

        public Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            _cache.Set(cacheKey, cacheValue, new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expiration
            });

            return Task.CompletedTask;
        }
    }
}
