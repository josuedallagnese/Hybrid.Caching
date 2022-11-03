using System.Threading.Tasks;

namespace Hybrid.Caching.State
{
    public interface IHybridCacheStateNotifier
    {
        Task NotifyChangesAsync(CacheState state);
    }
}
