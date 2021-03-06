﻿using System;
using Microsoft.Extensions.Caching.Distributed;

namespace Es.Throttle.Mvc
{
    public class RateLimitStoreDistributed : IRateLimitStore
    {
        private readonly IDistributedCache _memoryCache;

        private static readonly TimeSpan Sec = TimeSpan.FromSeconds(1);

        public RateLimitStoreDistributed(IDistributedCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool TryGet(string key, out long timestampMs)
        {
            timestampMs = -1;
            var data = _memoryCache.Get(key);
            if (data == null || data.Length == 0)
                return false;
            timestampMs = BitConverter.ToInt64(data, 0);
            return true;
        }

        public bool Add(string key, long timestampMs, TimeSpan ttl)
        {
            var data = _memoryCache.Get(key);
            if (data != null && data.Length > 0)
                return false;
            if (ttl.Ticks < 1)
            {
                ttl = Sec;
            }
            _memoryCache.Set(key, BitConverter.GetBytes(timestampMs),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                });
            return true;
        }

        public bool Update(string key, long timestampMs, TimeSpan ttl)
        {
            var data = _memoryCache.Get(key);
            if (data == null || data.Length == 0)
                return false;
            if (ttl.Ticks < 1)
            {
                ttl = Sec;
            }
            _memoryCache.Set(key, BitConverter.GetBytes(timestampMs),
                 new DistributedCacheEntryOptions
                 {
                     AbsoluteExpirationRelativeToNow = ttl
                 });
            return true;
        }
    }
}