using ED47.BusinessAccessLayer.BusinessEntities;
using Ninject;

namespace ED47.BusinessAccessLayer
{
    public static class FileRepositoryFactory
    {
        private static IFileRepository _default;
        public static IFileRepository Default { get { return _default ?? (_default = BusinessComponent.Kernel.Get<IFileRepository>()); } }
    }

    public static class FileRepository
    {
        public static IFile CreateNewFile(string name, string businessKey, int? groupId = 0, bool requiresLogin = true,
            string langId = null, bool encrypted = false)
        {
            return FileRepositoryFactory.Default.CreateNewFile(name, businessKey, groupId, requiresLogin, langId,encrypted);
        }
    }
}
