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
        IFile CreateNewFile(string name, string businessKey, int? groupId = 0, bool requiresLogin = true, string langId = null, bool encrypted = false);
        IFile GetFileByKey(string businessKey, int? version = null);
        string GetUrl(IFile file, bool specificVersion = true);
    }
}
