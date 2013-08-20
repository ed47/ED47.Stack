using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ED47.BusinessAccessLayer.Couchbase;

namespace ED47.BusinessAccessLayer.BusinessEntities.CouchBase
{
    public class EmailAttachment : BaseDocument
    {
        public File File { get; set; }
    }
}
