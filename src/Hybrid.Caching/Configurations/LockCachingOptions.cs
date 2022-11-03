using Microsoft.Extensions.Configuration;
using System;

namespace Hybrid.Caching.Configurations
{
    public class LockCachingOptions
    {
        /// <summary>
        /// Cache lock default expiration. Default is 30 seconds.
        /// </summary>
        public TimeSpan Expiry { get; set; }
        /// <summary>
        /// Cache lock default wait time. Default is 10 seconds.
        /// </summary>
        public TimeSpan Wait { get; set; }
        /// <summary>
        /// Cache lock default retry time. Default is 1 second.
        /// </summary>
        public TimeSpan Retry { get; set; }

        public LockCachingOptions()
        {
        }

        public LockCachingOptions(IConfigurationSection configurationSection)
        {
            Expiry = CachingOptions.GetValueOrDefault(configurationSection, nameof(Expiry), TimeSpan.FromSeconds(30));
            Wait = CachingOptions.GetValueOrDefault(configurationSection, nameof(Wait), TimeSpan.FromSeconds(10));
            Retry = CachingOptions.GetValueOrDefault(configurationSection, nameof(Retry), TimeSpan.FromSeconds(1));
        }
    }
}
