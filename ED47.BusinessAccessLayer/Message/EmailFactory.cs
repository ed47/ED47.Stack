using Ninject;

namespace ED47.BusinessAccessLayer.Message
{
    public static class EmailFactory
    {
        private static IEmailRepository _default;
        public static IEmailRepository Default
        {
            get { return _default ?? (_default = BusinessComponent.Kernel.Get<IEmailRepository>()); }
        }
    }
}
