using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ED47.BusinessAccessLayer.Couchbase;
using Newtonsoft.Json;

namespace ED47.BusinessAccessLayer.BusinessEntities.CouchBase.Comment
{
    public class FileBoxItem :BaseDocument,IFileBoxItem
    {
        public static FileBoxItem CreateNew(int fileBoxId, IFile file, string comment = null)
        {
            var fileBoxItem = new FileBoxItem
            {
                FileBoxId = fileBoxId,
                FileId = file.Id,
                Name = file.Name,
                FileExtension = Path.GetExtension(file.Name),
                Comment = comment
            };
            fileBoxItem.Save();
            return fileBoxItem;
        }

        public string Name { get; set; }

        public string FileExtension { get; set; }

        public string Comment { get; set; }

        public int FileBoxId { get; set; }

        public int FileId { get; set; }


        private IFile _file;

        [JsonIgnore]
        public IFile File
        {
            get { return _file ?? (_file = CouchBase.File.Get(FileId)); }
        }

        public IFile LoadFile()
        {
            return CouchBase.File.Get(FileId);
        }

        public new void Delete()
        {
            base.Delete();
        }
    }
}
