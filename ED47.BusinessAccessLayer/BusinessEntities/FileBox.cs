using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Web;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class FileBox : BusinessEntity, IFileBox
    {
        public int Id { get; set; }

        [MaxLength(250)]
        public virtual string ParentTypeName { get; set; }

        [MaxLength(500)]
        public virtual string Path { get; set; }

        public Guid Guid { get; set; }

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

        public static FileBox CreateNew(string parentTypeName, string path = null)
        {
            var fileBox = new FileBox
            {
                ParentTypeName = parentTypeName,
                Path = path
            };

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

        public FileBoxItem AddFile(HttpPostedFileBase file, string businessKey, int? groupdId = null, string comment = null, string langId = null, bool requireLogin = true, dynamic metadata = null, string name = null)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            if (!FileRepositoryFactory.Default.CheckIsSafe(file.FileName))
                return null;

        
            var newFile = FileRepository.CreateNewFile( System.IO.Path.GetFileName(file.FileName), businessKey, groupdId, requireLogin, langId, metadata: metadata);

            using (var fileStream = newFile.OpenWrite())
            {
                file.InputStream.CopyTo(fileStream);
                fileStream.Flush();
            }

            return FileBoxItem.CreateNew(Id, newFile, comment, name);
        }

        public FileBoxItem AddFile(IFile file, string comment = null)
        {
            return FileBoxItem.CreateNew(Id, file, comment);
        }

        public void Save()
        {
            BaseUserContext.Instance.Repository.Update<BusinessAccessLayer.Entities.FileBox, FileBox>(this);
        }

        public static IEnumerable<FileBox> Get(string root)
        {
            return BaseUserContext.Instance.Repository.Where<Entities.FileBox, FileBox>(el => !el.IsDeleted && el.Path.StartsWith(root));
        }

        public void Delete()
        {
            BaseUserContext.Instance.Repository.SoftDelete<Entities.FileBox>(Id);
        }
    }
}
