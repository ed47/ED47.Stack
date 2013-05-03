using System;
using Couchbase;
using Couchbase.Configuration;

namespace ED47.BusinessAccessLayer.Couchbase
{
public static class CouchbaseManager
{
   private readonly static CouchbaseClient _instance;

   static CouchbaseManager()
   {
       
       var conf = new CouchbaseClientConfiguration()
                      {
                          Bucket = "",
                          Password = "",
                          Username = "",
                          BucketPassword = "",
                          
                      };
       conf.Urls.Add(new Uri("http://127.0.0.1:8091/pools"));
       
       _instance = new CouchbaseClient(conf);
   }

   public static CouchbaseClient Instance { get { return _instance; } }
}
}
