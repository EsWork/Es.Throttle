using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Es.Throttle.Mvc
{
    public class RateLimitStoreMemory : IRateLimitStore
    {
        private readonly IMemoryCache _memoryCache;

        private static readonly TimeSpan Sec = TimeSpan.FromSeconds(1);

        public RateLimitStoreMemory(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool TryGet(string key, out long timestampMs)
        {
            timestampMs = -1;

            var data = _memoryCache.Get<long>(key);

            if (data == 0)
                return false;

            timestampMs = data;

            return true;
        }

        public bool Add(string key, long timestampMs, TimeSpan ttl)
        {
            var data = _memoryCache.Get<long>(key);

            if (data > 0)
                return false;

            if (ttl.Ticks < 1)
            {
                ttl = Sec;
            }

            _memoryCache.Set(key, timestampMs, ttl);

            return true;
        }

        public bool Update(string key, long timestampMs, TimeSpan ttl)
        {
            var data = _memoryCache.Get(key);

            if (data == null)
                return false;

            if (ttl.Ticks < 1)
            {
                ttl = Sec;
            }

            _memoryCache.Set(key, timestampMs, ttl);
            return true;
        }
    }
}