using System.Collections.Generic;
using System.Linq;

namespace Hybrid.Caching.Internal
{
    internal class DefaultCachingProvider : ICachingProvider
    {
        private readonly IEnumerable<ICache> _caches;

        public DefaultCachingProvider(IEnumerable<ICache> caches)
        {
            _caches = caches;
        }

        public ICache GetCache() => GetCache(CacheType.Hybrid);

        public ICache GetCache(CacheType type) => _caches.Single(s => s.Type == type);

        public IEnumerable<MetricResult> GetMetrics()
        {
            var memoryCache = GetCache(CacheType.Memory) as IMetriable;
            var redisCache = GetCache(CacheType.Redis) as IMetriable;

            var results = new List<MetricResult>()
            {
                memoryCache.GetMetrics().Get(),
                redisCache.GetMetrics().Get(),
            };

            return results;
        }
    }
}
