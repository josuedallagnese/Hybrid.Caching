using Microsoft.AspNetCore.Mvc;

namespace Hybrid.Caching.Web.Controllers
{
    [ApiController]
    [Route("api/hybrid")]
    public class HybridCacheController : ControllerBase
    {
        private readonly ICachingProvider _provider;

        public HybridCacheController(ICachingProvider provider)
        {
            _provider = provider;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var cache = _provider.GetCache(CacheType.Hybrid);

            var value = await cache.GetAsync<string>("key");

            return Ok(value);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] string value)
        {
            var cache = _provider.GetCache(CacheType.Hybrid);

            await cache.SetAsync("key", value);

            return Ok(value);
        }
    }
}
