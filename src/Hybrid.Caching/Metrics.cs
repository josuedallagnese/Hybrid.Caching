using System.Threading;
using Microsoft.Extensions.Logging;

namespace Hybrid.Caching
{
    public class Metrics
    {
        private readonly Counter _hits = new();
        private readonly Counter _misses = new();
        private readonly ILogger _logger;

        public Metrics(ILogger logger)
        {
            _logger = logger;
        }

        public void OnHit(string cacheKey)
        {
            _hits.Increment();

            _logger.LogInformation($"Cache hit = {cacheKey}");
        }

        public void OnMiss(string cacheKey)
        {
            _misses.Increment();

            _logger.LogInformation($"Cache missed = {cacheKey}");
        }

        public MetricResult Get() => new(_hits.Get(), _misses.Get());

        public sealed class Counter
        {
            private long _counter = 0;
            public void Increment() => Interlocked.Increment(ref _counter);
            public long Get() => Interlocked.Read(ref _counter);
        }
    }

    public record MetricResult(long Hits, long Misses);
}
