using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Hybrid.Caching.Configurations;

namespace Hybrid.Caching.Tests
{
    public class CachingOptionsTests
    {
        [Fact]
        public void CachingOptions_HybridCachingConfigurationSection_Should_ThrowException_WhenNotFound()
        {
            var configuration = new ConfigurationBuilder().Build();

            var exception = Assert.Throws<ArgumentNullException>(() => new CachingOptions(configuration));

            Assert.Equal("Value cannot be null. (Parameter 'Configuration section HybridCaching must be defined.')", exception.Message);
        }

        [Fact]
        public void CachingOptions_JsonSerializerOptions_Should_CreateObjectWithDefaultValues()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "HybridCaching:InstanceName", "Test" },
                    { "HybridCaching:Configuration", "localhost:6379,allowAdmin=true,abortConnect=false" },
                    { "HybridCaching:DefaultExpiration", "01:00:00" }
                }).Build();

            var options = new CachingOptions(configuration);

            Assert.NotNull(options.JsonSerializerOptions);
            Assert.Equal(options.JsonSerializerOptions.PropertyNamingPolicy, JsonNamingPolicy.CamelCase);
            Assert.Contains(options.JsonSerializerOptions.Converters, w => w is JsonStringEnumConverter);
        }

        [Fact]
        public void CachingOptions_LockingConfigurationSection_Should_CreateObjectWithDefaultValues()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "HybridCaching:InstanceName", "Test" },
                    { "HybridCaching:Configuration", "localhost:6379,allowAdmin=true,abortConnect=false" },
                    { "HybridCaching:DefaultExpiration", "01:00:00" }
                }).Build();

            var options = new CachingOptions(configuration);

            Assert.NotNull(options.Locking);
            Assert.Equal(options.Locking.Wait, TimeSpan.FromSeconds(10));
            Assert.Equal(options.Locking.Expiry, TimeSpan.FromSeconds(30));
            Assert.Equal(options.Locking.Retry, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void CachingOptions_DaprConfigurationSection_Should_CreateObjectWithDefaultValues()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "HybridCaching:InstanceName", "Test" },
                    { "HybridCaching:Configuration", "localhost:6379,allowAdmin=true,abortConnect=false" },
                    { "HybridCaching:DefaultExpiration", "01:00:00" }
                }).Build();

            var options = new CachingOptions(configuration);

            Assert.NotNull(options.Dapr);
            Assert.Equal("http://localhost:3500", options.Dapr.BaseUrl);
            Assert.Null(options.Dapr.ApiToken);
        }

        [Fact]
        public void CachingOptions_CanDefine_InstanceName()
        {
            var expectedValue = "Test";

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "HybridCaching:InstanceName", expectedValue }
                }).Build();

            var options = new CachingOptions(configuration);

            Assert.Equal(expectedValue, options.InstanceName);
        }

        [Fact]
        public void CachingOptions_CanDefine_Configuration()
        {
            var expectedValue = "Test";

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "HybridCaching:Configuration", expectedValue }
                }).Build();

            var options = new CachingOptions(configuration);

            Assert.Equal(expectedValue, options.Configuration);
        }

        [Fact]
        public void CachingOptions_CanDefine_DefaultExpiration()
        {
            var expectedValue = TimeSpan.FromHours(5);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "HybridCaching:DefaultExpiration", expectedValue.ToString() }
                }).Build();

            var options = new CachingOptions(configuration);

            Assert.Equal(expectedValue, options.DefaultExpiration);
        }
    }
}
