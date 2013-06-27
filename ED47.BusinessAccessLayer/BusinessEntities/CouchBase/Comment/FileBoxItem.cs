using System;
using System.IO;
using Newtonsoft.Json;

namespace ED47.BusinessAccessLayer.BusinessEntities.CouchBase.Comment
{
    public class FileBoxItem :IFileBoxItem
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string FileExtension { get; set; }

        public string Comment { get; set; }

        public int FileId { get; set; }

        public DateTime CreationDate { get; set; }

        public bool IsDeleted { get; set; }

        private IFile _file;

        [JsonIgnore]
        public IFile File
        {
            get { return _file ?? (_file = CouchBase.File.Get(FileId)); }
        }

        public static FileBoxItem CreateNew(IFile file, string comment = null)
        {
            var fileBoxItem = new FileBoxItem
            {
                Id = Guid.NewGuid().ToString(),
                FileId = file.Id,
                Name = file.Name,
                FileExtension = Path.GetExtension(file.Name),
                Comment = comment,
                CreationDate = DateTime.UtcNow
            };
            return fileBoxItem;
        }
        
        public IFile LoadFile()
        {
            return CouchBase.File.Get(FileId);
        }

        public bool Delete()
        {
            if (CanDelete())
            {
                IsDeleted = true;
                return true;
            }
                
            return false;
        }

        public bool CanDelete()
        {
            return true;
        }

    }
}
