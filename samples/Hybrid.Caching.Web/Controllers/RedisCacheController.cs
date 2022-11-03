using Microsoft.AspNetCore.Mvc;

namespace Hybrid.Caching.Web.Controllers
{
    [ApiController]
    [Route("api/redis")]
    public class RedisCacheController : ControllerBase
    {
        private readonly ICachingProvider _provider;

        public RedisCacheController(ICachingProvider provider)
        {
            _provider = provider;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var cache = _provider.GetCache(CacheType.Redis);

            var value = await cache.GetAsync<string>("key");

            return Ok(value);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] string value)
        {
            var cache = _provider.GetCache(CacheType.Redis);

            await cache.SetAsync("key", value);

            return Ok(value);
        }
    }
}
