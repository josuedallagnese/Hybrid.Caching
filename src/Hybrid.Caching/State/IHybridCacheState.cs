using System.Threading.Tasks;

namespace Hybrid.Caching.State
{
    public interface IHybridCacheState
    {
        Task InvalidateCacheAsync(CacheState state);
    }
}
