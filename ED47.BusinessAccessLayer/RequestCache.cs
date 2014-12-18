using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace ED47.BusinessAccessLayer
{
    public static class RequestCache
    {

        public static Dictionary<string, object> _Cache = new Dictionary<string, object>(); 

        public static object Get(string key) 
        {
            if (HttpContext.Current == null)
            {
                object o;
                _Cache.TryGetValue(key, out o);

                return o;
                

            }

            var context = HttpContext.Current;
            return context.Items[key];
        }

        public static TObject Get<TObject>(string key) where TObject : class
        {
            return Get(key) as TObject;
        }

        public static void Store(string key, object value)
        {
            if (HttpContext.Current == null)
            {
               _Cache.Add(key, value);
                return;

            }

            HttpContext.Current.Items[key] = value;
        }

    }
}
