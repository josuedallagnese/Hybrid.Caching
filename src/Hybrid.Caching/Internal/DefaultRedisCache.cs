using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.Logging;
using Hybrid.Caching.Configurations;

namespace Hybrid.Caching.Internal
{
    internal class DefaultRedisCache : ICache, IMetriable
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly CachingOptions _options;
        private readonly ILockingProvider _lockingProvider;
        private readonly Metrics _metrics;

        public DefaultRedisCache(
            CachingOptions options,
            IConnectionMultiplexer connectionMultiplexer,
            ILockingProvider lockingProvider,
            ILogger<DefaultRedisCache> logger)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _lockingProvider = lockingProvider;
            _options = options;

            _metrics = new Metrics(logger);
        }

        public CacheType Type { get; } = CacheType.Redis;

        public Task<bool> ExistsAsync(string cacheKey)
        {
            var database = _connectionMultiplexer.GetDatabase();

            return database.KeyExistsAsync(cacheKey);
        }

        public async Task<T> GetAsync<T>(string cacheKey)
        {
            var database = _connectionMultiplexer.GetDatabase();

            var rawValue = await database.StringGetAsync(cacheKey).ConfigureAwait(false);

            if (rawValue.IsNull)
            {
                _metrics.OnMiss(cacheKey);

                return default;
            }

            return Deserialize<T>(rawValue);
        }

        public Task<T> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever)
        {
            return GetAsync(cacheKey, dataRetriever, _options.DefaultExpiration);
        }

        public async Task<T> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            var database = _connectionMultiplexer.GetDatabase();

            var rawValue = await database.StringGetAsync(cacheKey).ConfigureAwait(false);

            // First check
            if (rawValue.HasValue)
            {
                _metrics.OnHit(cacheKey);

                return Deserialize<T>(rawValue);
            }

            return await _lockingProvider.LockAsync($"{cacheKey}:lock",
                () => InternalGet<T>(database, cacheKey, dataRetriever, expiration));
        }



        public async Task<TimeSpan?> GetExpirationAsync(string cacheKey)
        {
            var database = _connectionMultiplexer.GetDatabase();

            var timeSpan = await database.KeyTimeToLiveAsync(cacheKey);

            return timeSpan;
        }

        public Metrics GetMetrics() => _metrics;

        public Task RemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            var database = _connectionMultiplexer.GetDatabase();

            var redisKeys = cacheKeys.Select(s => (RedisKey)s).ToArray();

            return database.KeyDeleteAsync(redisKeys);
        }

        public Task RemoveAsync(string cacheKey)
        {
            var database = _connectionMultiplexer.GetDatabase();

            return database.KeyDeleteAsync(cacheKey);
        }

        public Task SetAsync<T>(string cacheKey, T cacheValue)
        {
            return SetAsync(cacheKey, cacheValue, _options.DefaultExpiration);
        }

        public Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            var database = _connectionMultiplexer.GetDatabase();

            var rawValue = Serialize(cacheValue);

            return database.StringSetAsync(cacheKey, rawValue, expiration);
        }


        private async Task<T> InternalGet<T>(IDatabase database, string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            // Double check after acquired lock
            var rawValue = await database.StringGetAsync(cacheKey).ConfigureAwait(false);

            if (rawValue.HasValue)
            {
                _metrics.OnHit(cacheKey);

                return Deserialize<T>(rawValue);
            }

            _metrics.OnMiss(cacheKey);

            T data;

            if (rawValue.IsNull)
            {
                data = await dataRetriever();

                rawValue = Serialize(data);

                await database.StringSetAsync(cacheKey, rawValue, expiration).ConfigureAwait(false);
            }
            else
            {
                data = Deserialize<T>(rawValue);
            }

            return data;
        }

        private T Deserialize<T>(string raw)
        {
            return JsonSerializer.Deserialize<T>(raw, _options.JsonSerializerOptions);
        }

        private string Serialize<T>(T data) => JsonSerializer.Serialize<T>(data, _options.JsonSerializerOptions);
    }
}
