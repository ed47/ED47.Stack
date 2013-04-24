using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer.BusinessEntities.CouchBase
{
    public class File
    {
        public string Name { get; set; }
        public Stream OpenRead()
        {
            return null;
        }
    }
}
