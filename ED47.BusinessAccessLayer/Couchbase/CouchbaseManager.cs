using System;
using System.Configuration;
using Couchbase;
using Couchbase.Configuration;
using Couchbase.Management;

namespace ED47.BusinessAccessLayer.Couchbase
{
public static class CouchbaseManager
{
   private readonly static CouchbaseClient _instance;

   static CouchbaseManager()
   {

       /*var config =  ConfigurationManager.GetSection("couchbase") as CouchbaseClientSection;
      
       
       var conf = new CouchbaseClientConfiguration()
                      {
                          Bucket = ConfigurationManager.AppSettings["CouchbaseBucket"],
                          Password = ConfigurationManager.AppSettings["CouchbasePassword"],
                          Username = ConfigurationManager.AppSettings["CouchbaseUsername"],
                          BucketPassword = ConfigurationManager.AppSettings["CouchbaseBucketPassword"],
                          
                      };
       conf.Urls.Add(new Uri("http://127.0.0.1:8091/pools"));*/
       
//       _instance = new CouchbaseClient((CouchbaseClientSection)ConfigurationManager.GetSection("couchbase"));
#if DEBUG
       _instance = new CouchbaseClient("couchbase"); // TODO: change to config section
#endif
#if !DEBUG
       _instance = new CouchbaseClient("couchbaseProd"); // TODO: change to config section
#endif
   }

   public static CouchbaseClient Instance { get { return _instance; } }
}
}
