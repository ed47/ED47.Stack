using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Helpers;


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
            var items = ContextItemCollection.GetItems();
            if (items  == null) return;

            items[key] = value;
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
            if (HttpContext.Current == null || !HttpContext.Current.User.Identity.IsAuthenticated) return null;
            
            if(HttpContext.Current.Session != null && HttpContext.Current.Session["Username"] != null)
                return HttpContext.Current.Session["Username"].ToString();
            
            return HttpContext.Current.User.Identity.Name;

        }

        public virtual string UserName
        { 
            get 
            { 
                return GetCurrentUserName();
            }   
        }

        protected abstract IRepository GetRepository();
      
      

        /// <summary>
        /// Gets the current context's Repository.
        /// </summary>
        public IRepository Repository
        {
            get { return GetRepository(); }
        }

        public static CacheItemPolicy DataCacheItemPolicy
        {
            get
            {
                return  new CacheItemPolicy {AbsoluteExpiration = DateTime.Now.AddMinutes(10)};
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

        private static string GetKey<TDbEntity>(IBusinessEntity entity) where TDbEntity : BaseDbEntity
        {
            return String.Join("&", entity.GetKeys<TDbEntity>().Select(el => el.Key + "=" + el.Value.ToString()));
        }

        public static TBusinessEntity GetStaticInstance<TDbEntity, TBusinessEntity>(int id, bool ignoreBusinessPredicate = false)
            where TDbEntity : DbEntity, IBaseDbEntity
            where TBusinessEntity : class, IBusinessEntity, new()
        {
            var key = typeof(TBusinessEntity).FullName + String.Format(".StaticInstance?id={0}", id);
            lock (key)
            {
                var cache = GetDataCache();

                var entity = cache.Get(key) as TBusinessEntity;
                if (entity == null)
                {
                    entity = Instance.Repository.Find<TDbEntity, TBusinessEntity>(el => el.Id == id, ignoreBusinessPredicate: ignoreBusinessPredicate);
                    if (entity == null)
                        return null;
                        
                    cache.Add(new CacheItem(key, entity), DataCacheItemPolicy);
                }
                return entity;
            }
        }

        public static void StoreDynamicInstance<TDbEntity, TBusinessEntity>(TBusinessEntity value)
            where TDbEntity : DbEntity, IBaseDbEntity
            where TBusinessEntity : IBusinessEntity, new()
        {
            var key = typeof(TBusinessEntity).FullName + String.Format(".DynamicInstance?id={0}", value.GetKeys<TDbEntity>().Single(el => el.Key == "Id").Value);
            lock (Lock.Get(key))
            {
                Store(key, value);
            }

        }


        public static void StoreDynamicInstance(Type businessEntityType, IBusinessEntity value)

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
            where TDbEntity : DbEntity, IBaseDbEntity
            where TBusinessEntity : IBusinessEntity, new()
        {
            foreach (var entity in values)
            {
                StoreDynamicInstance<TDbEntity, TBusinessEntity>(entity);
            }

        }

        public static TBusinessEntity GetDynamicInstance<TDbEntity, TBusinessEntity>(int id, bool ignoreBusinessPredicate = false)
            where TDbEntity : DbEntity, IBaseDbEntity
            where TBusinessEntity : class, IBusinessEntity, new()
        {
            var key = typeof(TBusinessEntity).FullName + String.Format(".DynamicInstance?id={0}", id);
            lock (Lock.Get(key))
            {
                var entity = Retrieve(key) as TBusinessEntity;
                if (entity == null)
                {
                    entity = Instance.Repository.Find<TDbEntity, TBusinessEntity>(el => el.Id == id, ignoreBusinessPredicate: ignoreBusinessPredicate);
                    if (entity == null) throw new NullReferenceException(String.Format("No entity {0} {1} found.", id, typeof(TBusinessEntity).FullName));
                    StoreDynamicInstance<TDbEntity, TBusinessEntity>(entity);
                }
                return entity;
            }
        }


        public static IBusinessEntity TryGetDynamicInstance(Type businessEntityType, int id)
           
        {
            var key = businessEntityType.FullName + String.Format(".DynamicInstance?id={0}", id);
            lock (Lock.Get(key))
            {
                return Retrieve(key) as IBusinessEntity;
            }
        }


     
    }
}
