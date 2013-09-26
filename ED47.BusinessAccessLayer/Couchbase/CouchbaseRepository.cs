using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase;
using Enyim.Caching.Memcached;
using Newtonsoft.Json;
using Omu.ValueInjecter;

namespace ED47.BusinessAccessLayer.Couchbase
{
    
    
    
    public class CouchbaseRepository
    {
        public static bool Load(IDocument document)
        {
            var client = CouchbaseManager.Instance;
            var op = client.ExecuteGet(document.GetKey());
            if (op.Success)
            {
                var tmp = JsonConvert.DeserializeObject(op.Value.ToString(), document.GetType());
                document.InjectFrom(tmp);
                document.Init();
                return true;
            }
            return false;
        }

        public static TDocument Get<TDocument>(string key) where TDocument : class, IDocument
        {
            var client = CouchbaseManager.Instance;
            var op = client.ExecuteGet(key);
            if (op.Success)
            {
                var o = JsonConvert.DeserializeObject<TDocument>(op.Value.ToString());
                o.Init();
                return o;
            }
            return null;
        }

        public static TDocument Get<TDocument>(object data) where TDocument : class, IDocument, new()
        {
            var client = CouchbaseManager.Instance;
           
            var res = new TDocument();
            res.InjectFrom(data);
            var op = client.ExecuteGet(res.GetKey());

            if (op.Success)
            {
                var o = JsonConvert.DeserializeObject<TDocument>(op.Value.ToString());
                o.Init();
                return o;
            }
            return null;
        }


        public static TDocument GetBy<TDocument>(string viewName, object value)
            where TDocument : class, IDocument, new()
        {
            return GetBy<TDocument>(typeof (TDocument).Name, viewName, value);
        }


        public static TDocument GetBy<TDocument>(string designName, string viewName, object value)
            where TDocument : class, IDocument, new()
        {
            var client = CouchbaseManager.Instance;
            var view = client.GetView(designName, viewName).Key(value);
            var item = view.Descending(true).FirstOrDefault();
            if (item == null) return null;
            var o = Get<TDocument>(item.ItemId);
            o.Init();
            return o;
        }


        public static bool Store(IDocument document)
        {
            var client = CouchbaseManager.Instance;
            if (string.IsNullOrEmpty(document.Key))
                document.Key = Guid.NewGuid().ToString();
            document.Type = document.GetType().Name;
            var jso = JsonConvert.SerializeObject(document);
            var res = client.Store(StoreMode.Set, document.GetKey(), jso);
            if (res)
                document.AfterSave();
            return res;
            
        }

        public static int GetNewId(string type)
        {
            var client = CouchbaseManager.Instance;
            return Convert.ToInt32(client.Increment(type, 1, 1));
        }

        public static bool Delete(IDocument document)
        {
            var client = CouchbaseManager.Instance;
            return client.ExecuteRemove(document.Key).Success;
        }

        public static IEnumerable<TDocument> GetAllBy<TDocument>(string designName, string viewName, int start = 0,  int count = 0) where TDocument : class, IDocument, new()
        {
            var client = CouchbaseManager.Instance;
            var view= client.GetView(designName, viewName).Skip(start).Stale(StaleMode.False);
            if (count > 0)
                view = view.Limit(count);
            var res = new List<TDocument>();
            foreach (var viewRow in view)
            {
                res.Add(Get<TDocument>(viewRow.ItemId));
            }
            
            return res;
        }

        public static IEnumerable<TDocument> GetByKey<TDocument>(string designName, string viewName, object startKey, object endKey = null, int limit = 1000, bool allowStale = false) where TDocument : class, IDocument, new()
        {
            var client = CouchbaseManager.Instance;
            var view = client.GetView(designName, viewName).StartKey(startKey).EndKey(endKey ?? startKey );
            if (limit > 0) view.Limit(limit);
            if (!allowStale) view.Stale(StaleMode.False);
            var res = new List<TDocument>();
            foreach (var viewRow in view)
            {
                res.Add(Get<TDocument>(viewRow.ItemId));
            }
            return res;
        }


        public static IEnumerable<TDocument> GetByKey<TDocument>(string designName, string viewName, string key, string startKey = null, string endKey = null, int limit = 1000, bool allowStale = false) where TDocument : class, IDocument
        {
            var client = CouchbaseManager.Instance;
            var view = key.Length != 0 ? client.GetView(designName, viewName).Key(key) : client.GetView(designName, viewName);
            if (limit > 0) view.Limit(limit);
            if (!allowStale) view.Stale(StaleMode.False);
            if (!string.IsNullOrEmpty(startKey)) view.StartKey(startKey);
            if (!string.IsNullOrEmpty(endKey)) view.EndKey(endKey);
            var res = new List<TDocument>();
            foreach (var viewRow in view)
            {
                res.Add(Get<TDocument>(viewRow.ItemId));
            }
            return res;
        }

        public static IEnumerable<TDocument> All<TDocument>(string type) where TDocument : class, IDocument
        {
            var client = CouchbaseManager.Instance;
            var view = client.GetView("all", "byType")
                                .StartKey(new[] { type, "z" })
                                .EndKey(new[] { type, "" })
                                .Descending(true);

            return view.Select(viewRow => Get<TDocument>(viewRow.ItemId)).ToList();
        }  

    }
}