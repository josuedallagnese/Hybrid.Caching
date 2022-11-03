using Microsoft.AspNetCore.Mvc;

namespace Hybrid.Caching.Web.Controllers
{
    [ApiController]
    [Route("api/metrics")]
    public class MetricsController : ControllerBase
    {
        private readonly ICachingProvider _provider;

        public MetricsController(ICachingProvider provider)
        {
            _provider = provider;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var metrics = _provider.GetMetrics();

            return Ok(metrics);
        }
    }
}
