using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Hybrid.Caching.Configurations;
using Hybrid.Caching.State;

namespace Hybrid.Caching.Internal
{
    internal class DefaultHybridCache : ICache, IHybridCacheState
    {
        private readonly CachingOptions _options;
        private readonly DefaultMemoryCache _memoryCache;
        private readonly DefaultRedisCache _redisCache;
        private readonly ILogger _logger;

        private readonly IHybridCacheStateNotifier _stateNotifier;

        public DefaultHybridCache(
            CachingOptions options,
            DefaultMemoryCache memoryCache,
            DefaultRedisCache redisCache,
            IHybridCacheStateNotifier stateNotifier,
            ILogger<DefaultHybridCache> logger)
        {
            _options = options;
            _memoryCache = memoryCache;
            _redisCache = redisCache;
            _stateNotifier = stateNotifier;
            _logger = logger;
        }

        public CacheType Type { get; } = CacheType.Hybrid;

        public async Task<bool> ExistsAsync(string cacheKey)
        {
            bool result;

            try
            {
                result = await _redisCache.ExistsAsync(cacheKey).ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Redis error on Exists operation - [{cacheKey}]");
            }

            result = await _memoryCache.ExistsAsync(cacheKey).ConfigureAwait(false);

            return result;
        }

        public Task<T> GetAsync<T>(string cacheKey)
        {
            return InternalGetAsync<T>(cacheKey);
        }

        public Task<T> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever)
        {
            return GetAsync<T>(cacheKey, dataRetriever, _options.DefaultExpiration);
        }

        public Task<T> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            return InternalGetAsync(cacheKey, new InternalGetParameter<T>(dataRetriever, expiration));
        }

        public async Task RemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            try
            {
                await _redisCache.RemoveAllAsync(cacheKeys).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Redis error on RemoveAllAsync operation - [\"{string.Join(", ", cacheKeys)}]\"");
            }

            await _memoryCache.RemoveAllAsync(cacheKeys).ConfigureAwait(false);

            await _stateNotifier.NotifyChangesAsync(new CacheState(cacheKeys)).ConfigureAwait(false);
        }

        public async Task RemoveAsync(string cacheKey)
        {
            try
            {
                await _redisCache.RemoveAsync(cacheKey).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Redis error on RemoveAsync operation - [\"{cacheKey}\"]");
            }

            await _memoryCache.RemoveAsync(cacheKey).ConfigureAwait(false);

            await _stateNotifier.NotifyChangesAsync(new CacheState(cacheKey)).ConfigureAwait(false);
        }

        public Task SetAsync<T>(string cacheKey, T cacheValue)
        {
            return SetAsync(cacheKey, cacheValue, _options.DefaultExpiration);
        }

        public async Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            await _memoryCache.SetAsync(cacheKey, cacheValue, expiration).ConfigureAwait(false);

            try
            {
                await _redisCache.SetAsync(cacheKey, cacheValue, expiration).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Redis error on Set operation - [\"{cacheKey}\"]");
            }

            await _stateNotifier.NotifyChangesAsync(new CacheState(cacheKey)).ConfigureAwait(false);
        }

        public async Task InvalidateCacheAsync(CacheState eventMessage)
        {
            await _memoryCache.RemoveAllAsync(eventMessage.Keys).ConfigureAwait(false);

            _logger.LogInformation($"Cache state: removed values from local cache: \"{string.Join(",", eventMessage.Keys)}\"");
        }

        private async Task<T> InternalGetAsync<T>(string cacheKey, InternalGetParameter<T> parameter = null)
        {
            var cacheValue = await _memoryCache.GetAsync<T>(cacheKey).ConfigureAwait(false);

            if (cacheValue != null)
            {
                return cacheValue;
            }

            try
            {
                if (parameter == null)
                {
                    cacheValue = await _redisCache.GetAsync<T>(cacheKey).ConfigureAwait(false);
                }
                else
                {
                    cacheValue = await _redisCache.GetAsync<T>(cacheKey, parameter.DataRetriever, parameter.Expiration).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Redis error on Get operation - [\"{cacheKey}\"]");
            }

            if (cacheValue != null)
            {
                var currentExpiration = await _redisCache.GetExpirationAsync(cacheKey);

                if (currentExpiration.HasValue)
                    await _memoryCache.SetAsync(cacheKey, cacheValue, currentExpiration.Value);
            }

            return cacheValue;
        }
    }
}
