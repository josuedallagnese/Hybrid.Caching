using System.Threading.Tasks;
using Dapr.Client;
using Hybrid.Caching.Configurations;
using Hybrid.Caching.State;

namespace Hybrid.Caching.Dapr
{
    internal class DaprHybridCacheStateNotifier : IHybridCacheStateNotifier
    {
        private readonly CachingOptions _options;

        public DaprHybridCacheStateNotifier(CachingOptions options)
        {
            _options = options;
        }

        public async Task NotifyChangesAsync(CacheState state)
        {
            var clientBuilder = new DaprClientBuilder()
                .UseJsonSerializationOptions(_options.JsonSerializerOptions);

            if (!string.IsNullOrWhiteSpace(_options.Dapr.BaseUrl))
                clientBuilder.UseGrpcEndpoint(_options.Dapr.BaseUrl);

            if (!string.IsNullOrWhiteSpace(_options.Dapr.ApiToken))
                clientBuilder.UseDaprApiToken(_options.Dapr.ApiToken);

            using var client = clientBuilder.Build();

            await client.PublishEventAsync("hybrid-caching", "state", state);
        }
    }
}
