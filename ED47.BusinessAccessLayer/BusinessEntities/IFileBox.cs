using System;
using System.Collections.Generic;
using System.Web;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public interface IFileBox
    {
        string Id { get; set; }
        string ParentTypeName { get; set; }
        IEnumerable<IFileBoxItem> FilesBoxItems { get; set;    }
        void AddFile(HttpPostedFileBase file, string businessKey, int? groupdId = null, string comment = null, string langId = null, bool requireLogin = true);
        void AddFile(IFile file, string comment = null);
    }
}