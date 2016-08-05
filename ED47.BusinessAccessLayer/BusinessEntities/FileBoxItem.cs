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

        public virtual int? FileId { get; set; }

        public virtual DateTime CreationDate { get; set; }

        private static readonly string[] Includes = new[] { "File" };

        public string ReportingScope { get; set; }
        private IFile _file;

        public bool IsPublic { get; set; }

        public Guid Guid { get; set; }

        [JsonIgnore]
        public IFile File
        {
            get
            {
                if (!FileId.HasValue) return null;
                return _file ?? (_file = FileRepositoryFactory.Default.Get(FileId.Value));
            }
        }

        public bool IsFolder { get; set; }

        public int? FolderId { get; set; }

        public string CreatorUsername { get; set; }

        public static IEnumerable<FileBoxItem> GetByFileBoxId(int id)
        {
            return BaseUserContext.Instance.Repository
                    .Where<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(el => el.FileBoxId == id && !el.IsDeleted)
                    .OrderByDescending(el => el.CreationDate)
                    .ToList();
        }

        public static FileBoxItem CreateNew(int fileBoxId, IFile file, int? folderId = null, string comment = null, string name = null)
        {

            if (folderId.HasValue)
            {
                var folder = Get(folderId.Value);
                if (folder.FileBoxId != fileBoxId)
                    throw new ApplicationException("Invalid folder");
            }

            var fileBoxItem = new FileBoxItem()
            {
                FileBoxId = fileBoxId,
                FileId = file.Id,
                FolderId = folderId,
                Name = name ?? file.Name,
                FileExtension = Path.GetExtension(file.Name),
                Comment = comment
            };
            BaseUserContext.Instance.Repository.Add<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(fileBoxItem);
            return fileBoxItem;
        }

        public static FileBoxItem Get(int id)
        {
            return BaseUserContext.Instance.Repository.Find<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(el => el.Id == id);
        }

        public static FileBoxItem Get(FileBoxReference reference)
        {
            return BaseUserContext.Instance.Repository.Find<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(el => el.Id == reference.Id && el.Guid.ToString().Equals(reference.Token, StringComparison.InvariantCultureIgnoreCase));
        }

        public static FileBoxItem Get(int fileId, int fileBoxId)
        {
            return BaseUserContext.Instance.Repository.Where<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(el => el.FileId == fileId && el.FileBoxId == fileBoxId).FirstOrDefault();
        }

        public void Delete(bool soft = false, bool recursive = true)
        {
            if (soft)
            {
                BaseUserContext.Instance.Repository.SoftDelete<BusinessAccessLayer.Entities.FileBoxItem>(Id);
            }
            else
            {
                BaseUserContext.Instance.Repository.Delete<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(this);
            }

            if(IsFolder && recursive)
            {
                var fb = BaseUserContext.GetDynamicInstance<Entities.FileBox, FileBox>(FileBoxId);
                var children = fb.GetChildren(this, true);
                foreach (var item in children)
                {
                    item.Delete(soft);
                }
            }
        }


        public void Save()
        {

            BaseUserContext.Instance.Repository.Update<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(this);



        }

        public void MakePublic()
        {
            IsPublic = true;
            BaseUserContext.Instance.Repository.Update<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(this);

        }

        public void MakePrivate()
        {
            IsPublic = false;
            BaseUserContext.Instance.Repository.Update<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(this);

        }


    }
}
