using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ED47.BusinessAccessLayer.BusinessEntities.CouchBase.Comment
{
    public class FileBox : IFileBox
    {
        public string ParentTypeName { get; set; }
        [JsonIgnore]
        public IEnumerable<IFileBoxItem> _filesBoxItems { get; set; }
        public IEnumerable<IFileBoxItem> FilesBoxItems
        {
            get { return _filesBoxItems ?? (_filesBoxItems = new List<FileBoxItem>()); }
            set { _filesBoxItems = value; }
        }
        public string Id { get; set; }

        public void AddFile(HttpPostedFileBase file, string businessKey, int? groupdId = new int?(), string comment = null, string langId = null, bool requireLogin = true)
        {
            if (file == null || file.ContentLength == 0)
                return ;

            var newFile = File.CreateNewFile<File>(file.FileName, businessKey, groupdId, requireLogin, langId);

            using (var fileStream = newFile.OpenWrite())
            {
                file.InputStream.CopyTo(fileStream);
            }

            var fileBoxItem = FileBoxItem.CreateNew(newFile, comment);
            FilesBoxItems.ToList().Add(fileBoxItem);
        }

        public void AddFile(IFile file, string comment = null)
        {
            var fileBoxItem = FileBoxItem.CreateNew(file, comment);
            FilesBoxItems.ToList().Add(fileBoxItem);
        }

        public static IFileBox CreateNew(string parentTypeName)
        {
            var fileBox = new FileBox { Id = Guid.NewGuid().ToString(),ParentTypeName = parentTypeName };
            return fileBox;
        }

    }
}
