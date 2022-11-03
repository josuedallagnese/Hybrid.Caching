using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Threading.Tasks;
using Hybrid.Caching.Internal;
using Hybrid.Caching.Configurations;
using Hybrid.Caching.Dapr;
using Hybrid.Caching.State;

namespace Hybrid.Caching
{
    public static class CachingServiceCollectionExtensions
    {
        public static CachingConfigurationBuilder AddHybridCaching(this IServiceCollection services, IConfiguration configuration)
        {
            var options = new CachingOptions(configuration);

            return AddHybridCaching(services, options);
        }

        public static CachingConfigurationBuilder AddHybridCaching(this IServiceCollection services, CachingOptions options)
        {
            services.AddLogging();
            services.AddSingleton(options);

            services.AddMemoryCache();
            services.AddSingleton<DefaultMemoryCache>();
            services.AddSingleton<ICache, DefaultMemoryCache>();

            ConfigureRedis(services, options);

            services.AddSingleton<DefaultHybridCache>();
            services.AddSingleton<ICache, DefaultHybridCache>();

            services.AddSingleton<IHybridCacheState, DefaultHybridCache>();
            services.AddSingleton<IHybridCacheStateNotifier, DaprHybridCacheStateNotifier>();

            services.AddSingleton<ICachingProvider, DefaultCachingProvider>();

            return new CachingConfigurationBuilder(services, options);
        }

        private static void ConfigureRedis(IServiceCollection services, CachingOptions cachingOptions)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(cachingOptions.Configuration));

            services.AddStackExchangeRedisCache(config =>
            {
                config.ConnectionMultiplexerFactory = () =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    var connection = serviceProvider.GetService<IConnectionMultiplexer>();
                    return Task.FromResult(connection);
                };

                config.InstanceName = cachingOptions.InstanceName;
            });

            services.AddSingleton<DefaultRedisCache>();
            services.AddSingleton<ICache, DefaultRedisCache>();
            services.AddSingleton<ILockingProvider, RedisLockingProvider>();
        }
    }
}
