using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Hybrid.Caching.Configurations
{
    public class CachingConfigurationBuilder
    {
        public IServiceCollection Services { get; }
        public CachingOptions Options { get; }

        public CachingConfigurationBuilder(IServiceCollection services, CachingOptions options)
        {
            Services = services;
            Options = options;
        }

        public IServiceCollection WithJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
        {
            Options.ConfigureJsonSerializerOptions(jsonSerializerOptions);
            return Services;
        }
    }
}
