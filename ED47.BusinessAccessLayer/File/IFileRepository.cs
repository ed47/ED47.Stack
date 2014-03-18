using System.Collections.Generic;
using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.BusinessAccessLayer.File
{
    public interface IFileRepository
    {
        IFile Get(int fileId);
        IFile CreateNewFile(string name, string businessKey, int? groupId = 0, bool requiresLogin = true, string langId = null, bool encrypted = false, int? fileBoxId = null);
        IFile GetFileByKey(string businessKey, int? version = null);
        IEnumerable<IFile> GetHistoryFilesByKey(string businessKey);
        string GetUrl(IFile file, bool specificVersion = true);
        void RemoveFile(int id, int? fileBoxid);
        IEnumerable<IFile> GetByFileBox(int fileBoxId);
        bool CheckIsSafe(string filename);
    }
}
