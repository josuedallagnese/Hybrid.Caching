using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hybrid.Caching.Dapr;

namespace Hybrid.Caching.Configurations
{
    public class CachingOptions
    {
        public const string ConfigurationSectionName = "HybridCaching";

        /// <summary>
        /// Redis connection string. For more details <see href="https://stackexchange.github.io/StackExchange.Redis/Configuration.html"/>
        /// </summary>
        public string Configuration { get; set; }

        public string InstanceName { get; set; }

        /// <summary>
        /// Cache default expiration. Default is 1 hour.
        /// </summary>
        public TimeSpan DefaultExpiration { get; set; }

        /// <summary>
        /// Cache lock options.
        /// </summary>
        public LockCachingOptions Locking { get; set; }

        /// <summary>
        /// Dapr options.
        /// </summary>
        public DaprOptions Dapr { get; set; }

        public JsonSerializerOptions JsonSerializerOptions { get; private set; }

        public CachingOptions(IConfiguration configuration)
        {
            var sectionName = configuration.GetSection(ConfigurationSectionName);
            if (!sectionName.Exists())
                throw new ArgumentNullException($"Configuration section {ConfigurationSectionName} must be defined.");

            Configuration = sectionName[nameof(Configuration)];
            InstanceName = sectionName[nameof(InstanceName)];
            DefaultExpiration = GetValueOrDefault(sectionName, nameof(DefaultExpiration), TimeSpan.FromHours(1));
            Locking = new LockCachingOptions(configuration.GetSection(nameof(Locking)));
            Dapr = new DaprOptions(configuration.GetSection(nameof(Dapr)));

            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        internal static TimeSpan GetValueOrDefault(IConfigurationSection configurationSection, string name, TimeSpan defaultValue)
        {
            var defaultExpirationValue = configurationSection[name];

            if (!string.IsNullOrWhiteSpace(defaultExpirationValue))
            {
                if (TimeSpan.TryParse(defaultExpirationValue, out var @value))
                    return @value;
            }

            return defaultValue;
        }

        internal void ConfigureJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions) => JsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
    }
}
