using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hybrid.Caching.State
{
    public interface IHybridCacheState
    {
        Task InvalidateCacheAsync(IEnumerable<string> keys);
    }
}
