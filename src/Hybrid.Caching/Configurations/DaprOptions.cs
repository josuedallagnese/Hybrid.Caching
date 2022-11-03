using Microsoft.Extensions.Configuration;

namespace Hybrid.Caching.Dapr
{
    public class DaprOptions
    {
        /// <summary>
        /// Defaul Dapr sidecar url http://localhost:3500
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:3500";
        public string ApiToken { get; set; }

        public DaprOptions(IConfigurationSection configuration)
        {
            if (configuration.Exists())
            {
                BaseUrl = configuration.GetValue<string>(nameof(BaseUrl));
                ApiToken = configuration.GetValue<string>(nameof(ApiToken));
            }
        }
    }
}
