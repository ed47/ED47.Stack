using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.BusinessAccessLayer.File
{
    public abstract class FileRepository : IFileRepository
    {
        public abstract IFile Get(int fileId);

        public abstract IFile CreateNewFile(string name, string businessKey, int? groupId, bool requiresLogin = true, string langId = null, bool encrypted = false, int? fileBoxId = null, string metadata = null);
        public abstract IFile GetFileByKey(string businessKey, int? version = null);
        public abstract IEnumerable<IFile> GetHistoryFilesByKey(string businessKey);

        /// <summary>
        /// Gets the download URL for a file.
        /// </summary>
        /// <param name="file">The file reference.</param>
        /// <param name="specificVersion">If true, a specific version is fetched [default]. If false, latest version will be downloaded.</param>
        /// <returns>The URL to download the file.</returns>
        public string GetUrl(IFile file, bool specificVersion = true)
        {
            if (specificVersion)
                return String.Format("/fileRepository.axd?id={0}&token={1}", file.Id, file.Guid);
            return String.Format("/fileRepository.axd?key={0}&token={1}", file.BusinessKey, file.Guid);
        }

        public abstract void RemoveFile(int id, int? fileBoxid);
        public IEnumerable<IFile> GetByFileBox(int fileBoxId)
        {
            var fileBox = FileBox.Get(fileBoxId);

            if (fileBox == null)
                return new List<IFile>();

            return fileBox.GetFiles().Select(el => el.File);
        }

        public bool CheckIsSafe(string filename)
        {
            FileSecurity security;

            if (!FileSecurity.TryParse(ConfigurationManager.AppSettings["LocalFileRepository.Security"], out security))
                return true;

            switch (security)
            {
                case FileSecurity.WhiteList:
                    var whiteList = GetFileExtensionWhiteLists();
                    return whiteList.Contains(Path.GetExtension(filename.ToLowerInvariant()));
                    
            }

            return false;
        }

        public abstract IEnumerable<string> GetFileExtensionWhiteLists();
    }
}
