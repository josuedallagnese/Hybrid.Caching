using System;
using System.Collections.Generic;

namespace Hybrid.Caching.State
{
    public class CacheState
    {
        public IEnumerable<string> Keys { get; set; }

        public CacheState()
        {
        }

        public CacheState(string cacheKey)
        {
            Keys = new List<string>()
            {
                cacheKey
            };
        }

        public CacheState(IEnumerable<string> cacheKeys)
        {
            Keys = cacheKeys;
        }
    }
}
