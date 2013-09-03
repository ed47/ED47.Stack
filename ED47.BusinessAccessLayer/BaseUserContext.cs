using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Helpers;
using ED47.BusinessAccessLayer;


namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Class that manages access to the current context's Repository.
    /// </summary>
    public abstract class BaseUserContext
    {
        public static Func<BaseUserContext> CreateDefaultContext { get; set; } 

        public static string ApplicationUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["ApplicationUrl"];
               
            }
        }

        protected const string InstanceKey = "ED47.GrcTool.BaseUserContext.Instance";
      
        /// <summary>
        /// Retrieves a value from the context.
        /// </summary>
        /// <param name="key">The value's key.</param>
        /// <returns></returns>
        public static object Retrieve(string key)
        {
            if(HttpContext.Current == null) return null;
            
            var val = HttpContext.Current.Items[key];

           

            return val;
        }

        public virtual dynamic RetrieveFromCookie(string key)
        {
            var cookie = HttpContext.Current.Request.Cookies["ED47.UserContext"];
            if (cookie == null || cookie.Values[key] == null) return null;
            var o = Json.Decode(cookie.Values[key]);
            return o;
        }

        public virtual void StoreInCookie(string key, object value)
        {
            var cookie = HttpContext.Current.Request.Cookies["ED47.UserContext" ] ?? new HttpCookie("ED47.UserContext");
            var json = Json.Encode(value);
            cookie.Value = json;
            cookie.Expires = DateTime.Now.AddDays(1);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Stores a value from the context.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void Store(string key, object value)
        {
            if (HttpContext.Current == null) return;

            HttpContext.Current.Items[key] = value;


        }

        public event EventHandler Commited;

        /// <summary>
        /// Saves and commits data to the database.
        /// </summary>
        public virtual void Commit()
        {
            Repository.Commit();
            if (Commited != null)
                Commited(this, new EventArgs());
        }

        protected virtual string GetCurrentUserName()
        {
            if (HttpContext.Current == null || HttpContext.Current.User == null || !HttpContext.Current.User.Identity.IsAuthenticated) return null;
            return HttpContext.Current.User.Identity.Name;

        }

        public virtual string UserName
        { 
            get 
            { 
                return GetCurrentUserName();
            }   
        }

        protected abstract Repository GetRepository();
      
      

        /// <summary>
        /// Gets the current context's Repository.
        /// </summary>
        protected internal Repository Repository
        {
            get { return GetRepository(); }
        }

        public static CacheItemPolicy DataCacheItemPolicy
        {
            get
            {
                return  new CacheItemPolicy(){AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(10)};
            }
        }

        /// <summary>
        /// Maps a path relative to the application.
        /// </summary>
        /// <param name="path">The relative path to map.</param>
        /// <returns></returns>
        public string MapPath(string path)
        {
            if (HttpContext.Current == null)
                return null; //TODO: Maybe get the path from some other way

            return HttpContext.Current.Server.MapPath(path);
        }

        public static MemoryCache GetDataCache()
        {
            return MemoryCache.Default;
        }

        private static string GetKey<TDbEntity>(BusinessEntity entity) where TDbEntity : BaseDbEntity
        {
            return String.Join("&", entity.GetKeys<TDbEntity>().Select(el => el.Key + "=" + el.Value.ToString()));
        }

        public static TBusinessEntity GetStaticInstance<TDbEntity, TBusinessEntity>(int id)
            where TDbEntity : BaseDbEntity
            where TBusinessEntity : BusinessEntity, new()
        {
            var key = typeof(TBusinessEntity).FullName + String.Format(".StaticInstance?id={0}", id);
            lock (key)
            {
                var cache = GetDataCache();

                var entity = cache.Get(key) as TBusinessEntity;
                if (entity == null)
                {
                    entity = Instance.Repository.Find<TDbEntity, TBusinessEntity>(el => el.Id == id);
                    if (entity == null)
                        return null;
                        //throw new NullReferenceException(String.Format("No entity {0} {1} found.", id,
                        //                                               typeof(TBusinessEntity).FullName));
                    cache.Add(new CacheItem(key, entity), DataCacheItemPolicy);
                }
                return entity;
            }
        }

        public static void StoreDynamicInstance<TDbEntity, TBusinessEntity>(TBusinessEntity value)
            where TDbEntity : BaseDbEntity
            where TBusinessEntity : BusinessEntity, new()
        {
            var key = typeof(TBusinessEntity).FullName + String.Format(".DynamicInstance?id={0}", value.GetKeys<TDbEntity>().Single(el => el.Key == "Id").Value);
            lock (Lock.Get(key))
            {
                Store(key, value);
            }

        }


        public static void StoreDynamicInstance(Type businessEntityType, BusinessEntity value)

        {
            var prop = businessEntityType.GetProperty("Id");
            if (prop == null) return;

            var key = businessEntityType.FullName + String.Format(".DynamicInstance?id={0}",prop.GetValue(value,null));
            lock (Lock.Get(key))
            {
                Store(key, value);
            }

        }

        public static BaseUserContext Instance
        {
            get
            {
                var userContext = Retrieve(InstanceKey) as BaseUserContext;
                if (userContext == null && CreateDefaultContext != null)
                {
                    userContext = CreateDefaultContext();
                    Store(InstanceKey, userContext);
                }

                return userContext;
            }

        }


        public static void StoreDynamicInstances<TDbEntity, TBusinessEntity>(IEnumerable<TBusinessEntity> values)
            where TDbEntity : BaseDbEntity
            where TBusinessEntity : BusinessEntity, new()
        {
            foreach (var entity in values)
            {
                StoreDynamicInstance<TDbEntity, TBusinessEntity>(entity);
            }

        }

        public static TBusinessEntity GetDynamicInstance<TDbEntity, TBusinessEntity>(int id)
            where TDbEntity : BaseDbEntity
            where TBusinessEntity : BusinessEntity, new()
        {
            var key = typeof(TBusinessEntity).FullName + String.Format(".DynamicInstance?id={0}", id);
            lock (Lock.Get(key))
            {
                var entity = Retrieve(key) as TBusinessEntity;
                if (entity == null)
                {
                    entity = Instance.Repository.Find<TDbEntity, TBusinessEntity>(el => el.Id == id);
                    if (entity == null) throw new NullReferenceException(String.Format("No entity {0} {1} found.", id, typeof(TBusinessEntity).FullName));
                    StoreDynamicInstance<TDbEntity, TBusinessEntity>(entity);
                }
                return entity;
            }
        }


        public static BusinessEntity TryGetDynamicInstance(Type businessEntityType, int id)
           
        {
            var key = businessEntityType.FullName + String.Format(".DynamicInstance?id={0}", id);
            lock (Lock.Get(key))
            {
                return Retrieve(key) as BusinessEntity;
            }
        }
    }
}
