using Couchbase;

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
       _instance = new CouchbaseClient("couchbase"); // TODO: change to config section
   }

   public static CouchbaseClient Instance { get { return _instance; } }
}
}
