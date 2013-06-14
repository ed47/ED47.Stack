using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ED47.BusinessAccessLayer.Couchbase;

namespace ED47.BusinessAccessLayer.BusinessEntities.CouchBase.Comment
{
    public class FileBox : BaseDocument,IFileBox
    {
        public string ParentTypeName { get; set; }

        public IEnumerable<IFileBoxItem> GetFiles()
        {
            throw new NotImplementedException();
        }

        public IFileBoxItem AddFile(HttpPostedFileBase file, string businessKey, int? groupdId = new int?(), string comment = null, string langId = null, bool requireLogin = true)
        {
            throw new NotImplementedException();
        }

        public IFileBoxItem AddFile(IFile file, string comment = null)
        {
            throw new NotImplementedException();
        }

        public static IFileBox CreateNew(string comment)
        {
            throw new NotImplementedException();
        }

        public static IFileBox Get(int value)
        {
            throw new NotImplementedException();
        }
    }
}
