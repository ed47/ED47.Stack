using System;
using System.Collections.Generic;
using System.Linq;
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

        public static TDocument Get<TDocument>(string key) where TDocument : class, IDocument, new()
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
            var item = view.FirstOrDefault();
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
            var view= client.GetView(designName, viewName).Skip(start);
            if (count > 0)
                view = view.Limit(count);
            var res = new List<TDocument>();
            foreach (var viewRow in view)
            {
                res.Add(Get<TDocument>(viewRow.ItemId));
            }
            
            return res;
        }

    }
}