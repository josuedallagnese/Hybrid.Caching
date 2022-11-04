using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hybrid.Caching.State
{
    public interface IHybridCacheStateNotifier
    {
        Task NotifyChangesAsync(IEnumerable<string> keys);
    }
}
