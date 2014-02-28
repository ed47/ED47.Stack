using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.BusinessAccessLayer
{
    public static class FileRepository
    {
        public static IFile CreateNewFile(string name, string businessKey, int? groupId = 0, bool requiresLogin = true,
            string langId = null, bool encrypted = false, int? fileBoxId = null)
        {
            return FileRepositoryFactory.Default.CreateNewFile(name, businessKey, groupId, requiresLogin, langId, encrypted, fileBoxId);
        }
    }
}