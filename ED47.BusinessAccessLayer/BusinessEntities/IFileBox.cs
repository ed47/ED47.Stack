using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public interface IFileBox
    {
        int Id { get; set; }
        string ParentTypeName { get; set; }

        IEnumerable<IFileBoxItem> GetFiles();
        IFileBoxItem AddFile(HttpPostedFileBase file, string businessKey, int? groupdId = null, string comment = null, string langId = null, bool requireLogin = true);
        IFileBoxItem AddFile(IFile file, string comment = null);

        /// <summary>
        ///   Inits this instance. This method is executed after the database load and instance creation.
        /// </summary>
        void Init();
    }
}