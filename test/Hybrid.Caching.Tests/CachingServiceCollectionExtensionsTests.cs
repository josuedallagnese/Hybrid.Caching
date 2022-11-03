using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Hybrid.Caching.Configurations;

namespace Hybrid.Caching.Tests
{
    public class CachingServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHybridCaching_When_JsonSerializerOptionsIsDefined()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "HybridCaching:InstanceName", "Test" },
                    { "HybridCaching:Configuration", "localhost:6379,allowAdmin=true,abortConnect=false" },
                    { "HybridCaching:DefaultExpiration", "01:00:00" },
                    { "HybridCaching:Locking:Retry", "00:00:01" },
                    { "HybridCaching:Locking:Expiry", "00:00:30" },
                    { "HybridCaching:Locking:Wait", "00:00:10" }
                }).Build();

            var options = new CachingOptions(configuration);

            var services = new ServiceCollection()
                .AddHybridCaching(options)
                .WithJsonSerializerOptions(new System.Text.Json.JsonSerializerOptions()
                {
                    PropertyNamingPolicy = null
                });

            var container = services.BuildServiceProvider();

            var defaultOptions = container.GetRequiredService<CachingOptions>();

            Assert.Null(defaultOptions.JsonSerializerOptions.PropertyNamingPolicy);
        }

        [Fact]
        public void AddHybridCaching_When_JsonSerializerOptionsIsNotDefined()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "HybridCaching:InstanceName", "Test" },
                    { "HybridCaching:Configuration", "localhost:6379,allowAdmin=true,abortConnect=false" },
                    { "HybridCaching:DefaultExpiration", "01:00:00" },
                    { "HybridCaching:Locking:Retry", "00:00:01" },
                    { "HybridCaching:Locking:Expiry", "00:00:30" },
                    { "HybridCaching:Locking:Wait", "00:00:10" }
                }).Build();

            var options = new CachingOptions(configuration);

            var services = new ServiceCollection()
                .AddHybridCaching(options)
                .Services;

            var container = services.BuildServiceProvider();

            var defaultOptions = container.GetRequiredService<CachingOptions>();

            Assert.Equal(JsonNamingPolicy.CamelCase, defaultOptions.JsonSerializerOptions.PropertyNamingPolicy);
        }

        [Fact]
        public void AddHybridCaching_Should_RegisterAllDependencies()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "HybridCaching:InstanceName", "Test" },
                    { "HybridCaching:Configuration", "localhost:6379,allowAdmin=true,abortConnect=false" },
                    { "HybridCaching:DefaultExpiration", "01:00:00" },
                    { "HybridCaching:Locking:Retry", "00:00:01" },
                    { "HybridCaching:Locking:Expiry", "00:00:30" },
                    { "HybridCaching:Locking:Wait", "00:00:10" }
                }).Build();

            var options = new CachingOptions(configuration);

            var services = new ServiceCollection()
                .AddHybridCaching(options);

            var container = services.Services.BuildServiceProvider();

            var caches = container.GetServices<ICache>();

            Assert.Contains(caches, a => a.Type == CacheType.Memory);
            Assert.Contains(caches, a => a.Type == CacheType.Redis);
            Assert.Contains(caches, a => a.Type == CacheType.Hybrid);

            var cacheProvider = container.GetRequiredService<ICachingProvider>();

            Assert.NotNull(cacheProvider);
            Assert.NotNull(cacheProvider.GetCache(CacheType.Memory));
            Assert.NotNull(cacheProvider.GetCache(CacheType.Redis));
            Assert.NotNull(cacheProvider.GetCache(CacheType.Hybrid));

            Assert.Equal(CacheType.Hybrid, cacheProvider.GetCache().Type);

            var defaultCache = container.GetRequiredService<ICache>();

            Assert.Equal(CacheType.Hybrid, defaultCache.Type);
        }
    }
}
