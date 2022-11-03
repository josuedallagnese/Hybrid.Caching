using System.Collections.Generic;

namespace Hybrid.Caching
{
    public interface ICachingProvider
    {
        ICache GetCache();
        ICache GetCache(CacheType type);
        IEnumerable<MetricResult> GetMetrics();
    }
}
