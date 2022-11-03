using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using Hybrid.Caching.Configurations;
using Hybrid.Caching.Internal;
using Hybrid.Caching.State;
using Hybrid.Caching.Tests.Mocks;

namespace Hybrid.Caching.Tests
{
    public class HybridCacheTests
    {
        private readonly CachingOptions _options;
        private readonly Mock<IDatabase> _mockDatabase;
        private readonly Mock<IConnectionMultiplexer> _mockConnectionMultiplexer;
        private readonly Mock<IHybridCacheStateNotifier> _stateNotifier;
        private readonly DefaultMemoryCache _memoryCache;
        private readonly DefaultRedisCache _redisCache;
        private readonly DefaultHybridCache _hybridCache;

        public HybridCacheTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "HybridCaching:InstanceName", "Test" },
                    { "HybridCaching:Configuration", "localhost:6379,allowAdmin=true,abortConnect=false" },
                    { "HybridCaching:DefaultExpiration", "01:00:00" }
                }).Build();

            _options = new CachingOptions(configuration);

            // Arrange
            _mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();

            _mockConnectionMultiplexer.Setup(_ => _.IsConnected).Returns(false);

            _mockDatabase = new Mock<IDatabase>();

            _mockConnectionMultiplexer
                .Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(_mockDatabase.Object);
            _stateNotifier = new Mock<IHybridCacheStateNotifier>();

            _memoryCache = new DefaultMemoryCache(
                _options,
                new MemoryCache(new MemoryCacheOptions()),
                new LoggerFactory().CreateLogger<DefaultMemoryCache>());

            _redisCache = new DefaultRedisCache(
                _options,
                _mockConnectionMultiplexer.Object,
                new FakeLockingProvider(),
                new LoggerFactory().CreateLogger<DefaultRedisCache>());

            _hybridCache = new DefaultHybridCache(
                _options,
                _memoryCache,
                _redisCache,
                _stateNotifier.Object,
                new LoggerFactory().CreateLogger<DefaultHybridCache>());
        }

        [Fact]
        public async Task HybridCache_ExistsAsync_Should_UseRedisWithPriority()
        {
            var key = "Key";

            _mockDatabase
                .Setup(s => s.KeyExistsAsync(key, CommandFlags.None))
                .ReturnsAsync(true);

            var value = await _hybridCache.ExistsAsync(key);

            _mockDatabase.Verify(v => v.KeyExistsAsync(key, CommandFlags.None), Times.Once);

            Assert.True(value);
        }

        [Fact]
        public async Task HybridCache_ExistsAsync_Should_UseMemoryWithFallback()
        {
            var key = "Key";

            _mockDatabase
                .Setup(s => s.KeyExistsAsync(key, CommandFlags.None))
                .ThrowsAsync(new RedisTimeoutException("Timeout simulation", CommandStatus.Unknown));

            await _memoryCache.SetAsync(key, true);

            var value = await _hybridCache.ExistsAsync(key);

            _mockDatabase.Verify(v => v.KeyExistsAsync(key, CommandFlags.None), Times.Once);

            Assert.True(value);
        }

        [Fact]
        public async Task HybridCache_GetAsync_ReturnValue_FromMemory_When_Found_InMemoryCache()
        {
            var key = "Key";
            var expectedValue = "Value";

            await _memoryCache.SetAsync(key, expectedValue);

            var value = await _hybridCache.GetAsync<string>(key);

            _mockDatabase.Verify(v => v.StringGetAsync(key, CommandFlags.None), Times.Never);

            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public async Task HybridCache_GetAsync_ReturnValue_FromRedis_When_NotFound_InMemory_AndUpdateEntry()
        {
            var key = "Key";
            var expectedValue = "Value";

            _mockDatabase
                .Setup(s => s.StringGetAsync(key, CommandFlags.None))
                .ReturnsAsync(JsonSerializer.Serialize(expectedValue));

            _mockDatabase
                .Setup(s => s.KeyTimeToLiveAsync(key, CommandFlags.None))
                .ReturnsAsync(TimeSpan.FromSeconds(5));

            var value = await _hybridCache.GetAsync<string>(key);

            _mockDatabase.Verify(v => v.StringGetAsync(key, CommandFlags.None), Times.Once);
            _mockDatabase.Verify(v => v.KeyTimeToLiveAsync(key, CommandFlags.None), Times.Once);

            Assert.Equal(expectedValue, value);

            var expectedValueInMemory = await _memoryCache.GetAsync<string>(key);

            Assert.Equal(expectedValueInMemory, expectedValue);
        }

        [Fact]
        public async Task HybridCache_GetAsync_With_DataRetriever_ShouldSetValueInCaches()
        {
            var key = "Key";
            var expectedValue = "Value";
            var serializedExpectedValue = JsonSerializer.Serialize(expectedValue);
            var dataRetrieverCalled = false;

            _mockDatabase.Setup(s => s.KeyTimeToLiveAsync(key, CommandFlags.None))
                .ReturnsAsync(_options.DefaultExpiration);

            var value = await _hybridCache.GetAsync(key, () =>
            {
                dataRetrieverCalled = true;

                return Task.FromResult(expectedValue);
            });

            _mockDatabase.Verify(v => v.StringGetAsync(key, CommandFlags.None), Times.Exactly(2));
            _mockDatabase.Verify(v => v.StringSetAsync(key,
                serializedExpectedValue,
                _options.DefaultExpiration,
                When.Always, CommandFlags.None), Times.Once);

            _mockDatabase.Verify(v => v.KeyTimeToLiveAsync(key, CommandFlags.None), Times.Once);

            Assert.Equal(expectedValue, value);

            var expectedValueInMemory = await _memoryCache.GetAsync<string>(key);

            Assert.Equal(expectedValueInMemory, expectedValue);
            Assert.True(dataRetrieverCalled);
        }
    }
}
