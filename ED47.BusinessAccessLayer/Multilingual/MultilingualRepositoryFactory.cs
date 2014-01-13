using Ninject;

namespace ED47.BusinessAccessLayer.Multilingual
{
    public static class MultilingualRepositoryFactory
    {
        public static IMultilingualRepository Create()
        {
            return BusinessComponent.Kernel.Get<IMultilingualRepository>();
        }
    }
}