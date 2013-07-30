using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Web;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class FileBox : BusinessEntity
    {
      public int Id { get; set; }

        [MaxLength(250)]
        public virtual string ParentTypeName { get; set; }
        
        /// <summary>
        /// Property used to construct Where clause in business repository.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        protected static Expression<Func<BusinessAccessLayer.Entities.FileBox, bool>> Where
        {
            get
            {
                return e => !e.IsDeleted;
            }
        }
        
        public static FileBox Get(int id)
        {
            return BaseUserContext.Instance.Repository.Find<BusinessAccessLayer.Entities.FileBox, FileBox>(el => el.Id == id);
        }

        public static FileBox CreateNew(string parentTypeName)
        {
            var fileBox = new FileBox(){ ParentTypeName = parentTypeName };
            BaseUserContext.Instance.Repository.Add<BusinessAccessLayer.Entities.FileBox, FileBox>(fileBox);
            return fileBox;
        }

        /// <summary>
        /// Returns the count of files in a filebox.
        /// </summary>
        /// <param name="fileBoxId">The filebox's Id.</param>
        /// <returns></returns>
        public static int FileCount(int? fileBoxId)
        {
            if (!fileBoxId.HasValue) return 0;

            return BaseUserContext.Instance.Repository.Count<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(el => el.FileBoxId == fileBoxId);
        }

        public IEnumerable<FileBoxItem> GetFiles()
        {
            return FileBoxItem.GetByFileBoxId(Id);
        }

        public FileBoxItem AddFile(HttpPostedFileBase file, string businessKey, int? groupdId = null, string comment = null, string langId = null, bool requireLogin = true)
        {
            if(file == null || file.ContentLength == 0)
                return null;
            
            var newFile = File.CreateNewFile<File>(System.IO.Path.GetFileName(file.FileName), businessKey, groupdId, requireLogin, langId);

            using (var fileStream = newFile.OpenWrite())
            {
                file.InputStream.CopyTo(fileStream);
                fileStream.Flush();
            }

            return FileBoxItem.CreateNew(Id, newFile, comment);
        }

        public FileBoxItem AddFile(File file, string comment = null)
        {
            return FileBoxItem.CreateNew(Id, file, comment);
        }
    }
}
