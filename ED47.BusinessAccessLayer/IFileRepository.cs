using System.Collections.Generic;
using System.IO;
using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.BusinessAccessLayer
{
    public interface IFileRepository
    {
        bool Write(IFile file, byte[] content);
        bool Append(IFile file, byte[] content);
        bool Delete(IFile file);
        bool Exist(IFile file);
        Stream OpenWrite(IFile file);
        Stream OpenRead(IFile file);
        IFile Get(int fileId);
        IFile CreateNewFile(string name, string businessKey, int? groupId = 0, bool requiresLogin = true, string langId = null, bool encrypted = false, int? fileBoxId = null);
        IFile GetFileByKey(string businessKey, int? version = null);
        IEnumerable<IFile> GetHistoryFilesByKey(string businessKey);
        string GetUrl(IFile file, bool specificVersion = true);
        void RemoveFile(int id, int? fileBoxid);
        IEnumerable<IFile> GetByFileBox(int fileBoxId);
    }
}
