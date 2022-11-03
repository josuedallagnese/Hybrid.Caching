using System;
using System.Threading.Tasks;

namespace Hybrid.Caching.Internal
{
    internal class InternalGetParameter<T>
    {
        public Func<Task<T>> DataRetriever { get; }
        public TimeSpan Expiration { get; }

        public InternalGetParameter(Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            DataRetriever = dataRetriever;
            Expiration = expiration;
        }
    }
}
