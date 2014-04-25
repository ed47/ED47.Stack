using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class FileBoxItem : BusinessEntity
    {
        public virtual int Id { get; set; }

        [MaxLength(200)]
        public virtual string Name { get; set; }

        [MaxLength(10)]
        public virtual string FileExtension { get; set; }

        [MaxLength(500)]
        public virtual string Comment { get; set; }

        public virtual int FileBoxId { get; set; }

        public virtual int FileId { get; set; }

        public virtual DateTime CreationDate { get; set; }

        private static readonly string[] Includes = new[] { "File" };

        private IFile _File;

        [JsonIgnore]
        public IFile File
        {
            get { return _File ?? (_File = FileRepositoryFactory.Default.Get(FileId)); }
        }

        public string CreatorUsername { get; set; }

        public static IEnumerable<FileBoxItem> GetByFileBoxId(int id)
        {
            return BaseUserContext.Instance.Repository
                    .Where<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(el => el.FileBoxId == id && !el.IsDeleted)
                    .OrderByDescending(el => el.CreationDate)
                    .ToList();
        }

        public static FileBoxItem CreateNew(int fileBoxId, IFile file, string comment = null)
        {
            var fileBoxItem = new FileBoxItem()
            {
                FileBoxId = fileBoxId,
                FileId = file.Id,
                Name = file.Name,
                FileExtension = Path.GetExtension(file.Name),
                Comment = comment
            };
            BaseUserContext.Instance.Repository.Add<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(fileBoxItem);
            return fileBoxItem;
        }

        private IFile LoadFile()
        {
            return FileRepositoryFactory.Default.Get(FileId);
        }

        public static FileBoxItem Get(int id)
        {
            return BaseUserContext.Instance.Repository.Find<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(el => el.Id == id);
        }

        public static FileBoxItem Get(int fileId, int fileBoxId)
        {
            return BaseUserContext.Instance.Repository.Where<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(el => el.FileId == fileId && el.FileBoxId == fileBoxId).FirstOrDefault();
        }

        public void Delete()
        {
            BaseUserContext.Instance.Repository.Delete<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(this);
        }
    }
}
