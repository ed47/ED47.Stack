using System.Collections.Generic;
using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.BusinessAccessLayer
{
    public static class FileRepository
    {
        public static IFile CreateNewFile(string name, string businessKey, int? groupId = 0, bool requiresLogin = true,
            string langId = null, bool encrypted = false, int? fileBoxId = null)
        {
            if (!FileRepositoryFactory.Default.CheckIsSafe(name))
                return null;

            return FileRepositoryFactory.Default.CreateNewFile(name, businessKey, groupId, requiresLogin, langId, encrypted, fileBoxId);
        }

        public static void RemoveFile(int id, int? fileBoxid = null)
        {
            FileRepositoryFactory.Default.RemoveFile(id, fileBoxid);
        }

        public static IEnumerable<IFile> GetByFileBox(int fileBoxId)
        {
            return FileRepositoryFactory.Default.GetByFileBox(fileBoxId);
        }

        public static IFile Get(int fileId)
        {
            return FileRepositoryFactory.Default.Get(fileId);
        }
    }
}