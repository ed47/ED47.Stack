using System;
using System.Linq;
using System.Runtime.Caching;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Lock's access to objects using a key.
    /// </summary>
    /// <remarks>Migrated from using a static dictionary to MemoryCache with 15 minute sliding expiration.</remarks>
    public class Lock
    {
        private static readonly object AccessLock = new object();

        private readonly object _lockObject = new object();


        
        /// <summary>
        /// Gets an object on which to do a safe lock() unto to prevent simultaneous writes on the same item.
        /// </summary>
        /// <param name="key">The lock key.</param>
        /// <returns>An object on which to lock() to.</returns>
        public static object Get(string key)
        {
            lock (AccessLock)
            {
                var cacheKey = "Lock?key=" + key;
                var currentLock = MemoryCache.Default.Get(cacheKey) as Lock;

                if (currentLock == null)
                {
                    currentLock = new Lock();
                    MemoryCache.Default.Add(cacheKey, currentLock, 
                        new CacheItemPolicy 
                        { 
                            SlidingExpiration = new TimeSpan(0, 15, 0), 
                            Priority = CacheItemPriority.NotRemovable 
                        });
                }
                
                return currentLock._lockObject;
            }
        }


        public static object Get<TEntity>(BusinessEntity entity) where  TEntity : DbEntity
        {
            var key = String.Join("&",entity.GetKeys<TEntity>().Select(el=>el.Key + "=" + el.Value.ToString()));
            return Get(String.Format("{0}?{1}", typeof (TEntity).FullName, key));
        }
    }
}