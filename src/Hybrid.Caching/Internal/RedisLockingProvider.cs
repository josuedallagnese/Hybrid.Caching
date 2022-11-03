using System;
using System.Collections.Generic;
using RedLockNet.SERedis.Configuration;
using RedLockNet.SERedis;
using StackExchange.Redis;
using Hybrid.Caching.Configurations;
using System.Threading.Tasks;

namespace Hybrid.Caching.Internal
{
    internal class RedisLockingProvider : ILockingProvider
    {
        private readonly CachingOptions _options;
        private readonly Lazy<RedLockFactory> _lockFactory;

        public RedisLockingProvider(CachingOptions options, IConnectionMultiplexer connectionMultiplexer)
        {
            _options = options;

            _lockFactory = new Lazy<RedLockFactory>(() =>
            {
                var multiplexers = new List<RedLockMultiplexer>
                {
                    new RedLockMultiplexer(connectionMultiplexer)
                };

                return RedLockFactory.Create(multiplexers);
            });
        }

        public async Task<T> LockAsync<T>(string key, Func<Task<T>> operation)
        {
            using var @lock = await _lockFactory.Value.CreateLockAsync(
                key,
                _options.Locking.Expiry,
                _options.Locking.Wait,
                _options.Locking.Retry).ConfigureAwait(false);

            if (@lock.IsAcquired)
            {
                return await operation().ConfigureAwait(false);
            }

            throw new InvalidOperationException("Locking provider cannot acquired lock");
        }
    }
}
