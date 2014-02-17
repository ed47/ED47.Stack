using Ninject;

namespace ED47.BusinessAccessLayer.Multilingual
{
    public static class MultilingualRepositoryFactory
    {
        public static IMultilingualRepository Create()
        {
            return BusinessComponent.Kernel.Get<IMultilingualRepository>();
        }

        private static IMultilingualRepository _default;
        public static IMultilingualRepository Default
        {
            get { return _default ?? (_default = BusinessComponent.Kernel.Get<IMultilingualRepository>()); }
        }
    }
}