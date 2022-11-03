using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Hybrid.Caching.State;

namespace Hybrid.Caching
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseHybridCaching(this WebApplication app)
        {
            app.UseCloudEvents();
            app.MapSubscribeHandler();

            app.MapPost("/state", [Topic("hybrid-caching", "state")] async (
                [FromServices] IHybridCacheState cacheState,
                CacheState state) =>
            {
                await cacheState.InvalidateCacheAsync(state);

                return Results.Ok(state);
            }).ExcludeFromDescription();

            return app;
        }
    }
}
