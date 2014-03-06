using Ninject;

namespace ED47.BusinessAccessLayer
{
    public static class RepositoryManager<TRepository> where TRepository : class
    {
        private static readonly string StoreKey = typeof(TRepository).FullName;
        
        // ReSharper disable once StaticFieldInGenericType
        public static readonly StandardKernel Kernel = new StandardKernel();
        
        // ReSharper disable once StaticFieldInGenericType
        private static TRepository _current;

        public static TRepository Current
        {
            get
            {
                var items = ContextItemCollection.GetItems();

                if (items == null)
                    return _current ?? (_current = Kernel.Get<TRepository>());

                var repository = items[StoreKey] as TRepository;

                if (repository == null)
                {
                    repository = Kernel.Get<TRepository>();
                    items.Add(StoreKey, repository);
                }

                return repository;
            }
        }
    }
}