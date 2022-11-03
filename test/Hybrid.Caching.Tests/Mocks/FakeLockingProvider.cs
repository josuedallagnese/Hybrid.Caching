namespace Hybrid.Caching.Tests.Mocks
{
    internal class FakeLockingProvider : ILockingProvider
    {
        public Task<T> LockAsync<T>(string key, Func<Task<T>> operation)
        {
            return operation();
        }
    }
}
