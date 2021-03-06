﻿namespace ED47.BusinessAccessLayer.Azure
{
    /// <summary>
    /// 
    /// </summary>
    /// 
    
    public class StorageFileConfig
    {
        public StorageFileConfig()
        {
            ContentType = "application/octet-stream";
            CacheControl = "public";
            Replace = false;
            Gzip = false;
      
        }

        public bool Gzip { get; set; }
        public string ContentType { get; set; }
        public string CacheControl { get; set; }
        public string CacheExpire { get; set; }
        public string ContentEncoding { get; set; }
        public string ContentLanguage { get; set; }
        public bool Replace { get; set; }
    }
}