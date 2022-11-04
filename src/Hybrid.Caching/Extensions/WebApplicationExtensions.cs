using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Hybrid.Caching.State;
using System.Collections.Generic;

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
                IEnumerable<string> keys) =>
            {
                await cacheState.InvalidateCacheAsync(keys);

                return Results.Ok(keys);
            }).ExcludeFromDescription();

            return app;
        }
    }
}
