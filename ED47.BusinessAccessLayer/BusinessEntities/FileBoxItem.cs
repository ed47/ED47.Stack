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
        public  int Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(10)]
        public  string FileExtension { get; set; }

        [MaxLength(500)]
        public  string Comment { get; set; }

        public  int FileBoxId { get; set; }

        public  int FileId { get; set; }

        public  DateTime CreationDate { get; set; }

        private static readonly string[] Includes = new[] { "File" };

        private IFile _file;

        [JsonIgnore]
        public  IFile File
        {
            get { return _file ?? (_file = BusinessEntities.File.Get(FileId)); }
        }

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
            BaseUserContext.Instance.Repository.Add<Entities.FileBoxItem, FileBoxItem>(fileBoxItem);
            return fileBoxItem;
        }

        public IFile LoadFile()
        {
            return BaseUserContext.Instance.Repository.Find<Entities.File, File>(el => el.Id == FileId);
        }

        public static FileBoxItem Get(int id)
        {
            return BaseUserContext.Instance.Repository.Find<Entities.FileBoxItem, FileBoxItem>(el => el.Id == id);
        }

        public void Delete()
        {

            BaseUserContext.Instance.Repository.Delete<Entities.FileBoxItem, FileBoxItem>(this);
        }
    }
}
