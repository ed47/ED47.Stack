using System.IO;
using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.BusinessAccessLayer
{
    public interface IFileRepository
    {
        bool Write(IFile file,  byte[] content);
        bool Append(IFile file, byte[] content);
        bool Delete(IFile file);
        bool Exist(IFile file);
        Stream OpenWrite(IFile file);
        Stream OpenRead(IFile file);
    }
}
