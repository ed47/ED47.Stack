using ED47.BusinessAccessLayer.File;
using Ninject;

namespace ED47.BusinessAccessLayer
{
    public static class FileRepositoryFactory
    {
        private static IFileRepository _default;
        public static IFileRepository Default { get { return _default ?? (_default = BusinessComponent.Kernel.Get<IFileRepository>()); } }
    }
}
