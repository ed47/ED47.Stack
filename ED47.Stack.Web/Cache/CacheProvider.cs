using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace ED47.Stack.Web.Cache
{
    public class CacheProvider
    {
        private static ObjectCache Cache { get { return MemoryCache.Default; } }

        public static object Get(string key)
        {
            return Cache[key];
        }

        public static void Set(string key, object data, int cacheTime = 60)
        {
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheTime) };

            Cache.Add(new CacheItem(key, data), policy);
        }

        public static bool Contains(string key)
        {
            return Cache.Contains(key);
        }

        public static void Invalidate(string key)
        {
            Cache.Remove(key);
        }
        public static void Clear()
        {
            foreach (var item in Cache)
            {
                Cache.Remove(item.Key);
            }
        }
    }
}