using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public interface IFileBox
    {
        int Id { get; set; }

        [MaxLength(250)]
        string ParentTypeName { get; set; }

        [MaxLength(500)]
        string Path { get; set; }

        IEnumerable<FileBoxItem> GetFiles();
        FileBoxItem AddFile(HttpPostedFileBase file, string businessKey, int? groupdId = null, string comment = null, string langId = null, bool requireLogin = true);
        FileBoxItem AddFile(IFile file, string comment = null);
        void Save();
    }
}