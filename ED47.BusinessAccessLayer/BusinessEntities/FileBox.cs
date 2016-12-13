using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using Ninject.Planning.Targets;

namespace ED47.BusinessAccessLayer.BusinessEntities
{

    public class FileBoxReference
    {
        public int Id { get; set; }
        public String Token { get; set; }
    }

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
            return BaseUserContext.Instance.Repository.Find<Entities.FileBox, FileBox>(el => el.Id == id);
        }

         public static FileBox Get(FileBoxReference reference)
        {
            return BaseUserContext.Instance.Repository.Find<Entities.FileBox, FileBox>(el => el.Id == reference.Id && el.Guid.ToString().Equals(reference.Token, StringComparison.InvariantCultureIgnoreCase));
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

        private List<FileBoxItem> _items;
        private ILookup<int, FileBoxItem> _itemsByFolder;

        private IEnumerable<FileBoxItem> _getItems()
        {
            if (_items == null)
            {
                _items = new List<FileBoxItem>(FileBoxItem.GetByFileBoxId(Id));
                _filter();

            }
            return _items;
        }

        public static Func<FileBoxItem, bool> HasAccess { get; set; }

        private HashSet<FileBoxItem> _blockedFiles = new HashSet<FileBoxItem>();

        private void _filter()
        {
            if (HasAccess == null || _items.All(el => String.IsNullOrEmpty(el.ReportingScope)))
                return;

            var res = new List<FileBoxItem>();
            var noAccess = _blockedFiles;
            foreach (var item in _items.Where(el => el.IsFolder && !String.IsNullOrEmpty(el.ReportingScope)))
            {
                if (noAccess.Contains(item) || HasAccess(item))
                    continue;

                noAccess.Add(item);
                var files = GetChildren(item, true);
                foreach (var file in files)
                {
                    noAccess.Add(file);
                }
            }
            foreach (var item in _items.Where(el => !noAccess.Contains(el) && !el.IsFolder && !String.IsNullOrEmpty(el.ReportingScope)))
            {

                if (!HasAccess(item))
                {
                    noAccess.Add(item);
                }


            }
            _items = _items.Where(el => !noAccess.Contains(el)).ToList();
            _itemsByFolder = null;
        }

        private IEnumerable<FileBoxItem> _getFolderItems(FileBoxItem folder, bool recursive = false)
        {

            if (_itemsByFolder == null)
            {
                _itemsByFolder = _getItems().ToLookup(el => el.FolderId.HasValue ? el.FolderId.Value : 0, el => el);
            }

            var folders = folder != null ? new List<FileBoxItem>() { folder } : _getItems().Where(el => !el.FolderId.HasValue && el.IsFolder).ToList();
            var res = new List<FileBoxItem>();
            if (folder == null)
                res.AddRange(folders);

            while (folders.Any())
            {
                var current = folders.First();
                folders.RemoveAt(0);
                var items = _itemsByFolder[current.Id];
                if (recursive)
                    folders.AddRange(items.Where(el => el.IsFolder));
                res.AddRange(items);

            }

            return res;
        }

        public IEnumerable<FileBoxItem> GetChildren(FileBoxItem folder, bool recursive)
        {
            return _getFolderItems(folder, recursive);
        }


        public IEnumerable<FileBoxItem> GetFolders()
        {
            return _getItems().Where(el => el.IsFolder);
        }

          public FileBoxItem GetItem(int id)
        {
            return _getItems().FirstOrDefault(el => el.Id == id);
        }

        public IEnumerable<FileBoxItem> GetFiles()
        {
            return _getItems().Where(el => !el.IsFolder);
        }

        public FileBoxItem AddFile(HttpPostedFileBase file, string businessKey, int? groupdId = null, int? folderId = null, string comment = null, string langId = null, bool requireLogin = true, dynamic metadata = null, string name = null)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            if (!FileRepositoryFactory.Default.CheckIsSafe(file.FileName))
                return null;

            if (folderId.HasValue && folderId.Value != 0)
            {
                var folder = _getItems().FirstOrDefault(el => el.Id == folderId.Value);
                if (_blockedFiles.Contains(folder)) return null;
            }

            var newFile = FileRepository.CreateNewFile(System.IO.Path.GetFileName(file.FileName), businessKey, groupdId, requireLogin, langId, metadata: metadata);

            using (var fileStream = newFile.OpenWrite())
            {
                file.InputStream.CopyTo(fileStream);
                fileStream.Flush();
            }

            var res = FileBoxItem.CreateNew(Id, newFile, folderId.HasValue ? (folderId.Value == 0 ? (int?)null : folderId.Value) : null , comment, name);

            _items = null;
            return res;
        }

        public FileBoxItem AddFile(IFile file, int? folderId = null, string comment = null)
        {
            var res = FileBoxItem.CreateNew(Id, file, folderId, comment);
            _items = null;
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="folderPath">subfolders separated with '/'</param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public FileBoxItem AddFileToPath(IFile file, string folderPath = null, string comment = null)
        {
            if (string.IsNullOrEmpty(folderPath))
                return AddFile(file, (int?)null, comment);

            var subDirectories = folderPath.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            int? parentFolder = null;
            foreach (var subdir in subDirectories)
            {
                var folder = GetFolders().FirstOrDefault(f => f.FolderId == parentFolder && f.Name == subdir)
                    ?? AddFolder(subdir, parentFolder);
                parentFolder = folder.Id;
            }
            return AddFile(file, parentFolder, comment);
        }

        public FileBoxItem AddFolder(string name, int? parentFolder, string comment = null)
        {

            if (parentFolder.HasValue && parentFolder.Value != 0)
            {
                var folder = FileBoxItem.Get(parentFolder.Value);
                if (folder.FileBoxId != Id)
                    throw new ApplicationException("Invalid folder");
            }




            var fileBoxItem = new FileBoxItem()
            {
                FileBoxId = Id,
                Name = name,
                Comment = comment,
                IsFolder = true,
                FolderId = parentFolder.HasValue ? (parentFolder.Value == 0 ? (int?)null : parentFolder.Value) : null
            };

            BaseUserContext.Instance.Repository.Add<BusinessAccessLayer.Entities.FileBoxItem, FileBoxItem>(fileBoxItem);

            _items = null;

            return fileBoxItem;
        }



        public void MoveItem(int fileBoxItemId, int? targetFolderId)
        {
            var items = _getItems();
            var item = items.FirstOrDefault(el => el.Id == fileBoxItemId);

            if (item == null) return;

            if (item.IsFolder)
            {



            }
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
