using System;
using System.Threading.Tasks;

namespace Hybrid.Caching
{
    public interface ILockingProvider
    {
        Task<T> LockAsync<T>(string key, Func<Task<T>> operation);
    }
}
