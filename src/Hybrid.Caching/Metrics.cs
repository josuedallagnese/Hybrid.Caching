using System.Threading;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Hybrid.Caching
{
    public class Metrics
    {
        public class Counter
        {
            private readonly long[] _counters = new long[2];

            public void Increment(MetricType statsType)
            {
                Interlocked.Increment(ref _counters[(int)statsType]);
            }

            public long Get(MetricType statsType)
            {
                return Interlocked.Read(ref _counters[(int)statsType]);
            }
        }

        private const string KEY = "cache_metrics";
        private readonly ConcurrentDictionary<string, Counter> _counters = new ConcurrentDictionary<string, Counter>();
        private readonly ILogger _logger;

        public Metrics(ILogger logger)
        {
            _logger = logger;
        }

        public void OnHit(string cacheKey)
        {
            GetCounter().Increment(MetricType.Hit);

            _logger.LogInformation($"Cache hit = {cacheKey}");
        }

        public void OnMiss(string cacheKey)
        {
            GetCounter().Increment(MetricType.Missed);

            _logger.LogInformation($"Cache missed = {cacheKey}");
        }

        public long GetMetric(MetricType statsType)
        {
            return GetCounter().Get(statsType);
        }

        private Counter GetCounter()
        {
            if (!_counters.TryGetValue(KEY, out var counter))
            {
                counter = new Counter();

                if (_counters.TryAdd(KEY, counter))
                {
                    return counter;
                }

                return GetCounter();
            }

            return counter;
        }
    }

    public class MetricResult
    {
        public CacheType Type { get; set; }
        public long Misses { get; set; }
        public long Hits { get; set; }

        public MetricResult()
        {
        }

        public MetricResult(CacheType type, Metrics metrics)
        {
            Type = type;
            Misses = metrics.GetMetric(MetricType.Missed);
            Hits = metrics.GetMetric(MetricType.Hit);
        }
    }
}
