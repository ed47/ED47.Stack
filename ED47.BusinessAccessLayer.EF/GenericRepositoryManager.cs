using System.Data.Entity;
using Ninject;
using Ninject.Parameters;

namespace ED47.BusinessAccessLayer.EF
{
    public class GenericRepositoryManager : BusinessAccessLayer.EF.IGenericRepositoryManager
    {
        private const string GenericRepositoryKey = "ED47.BusinessAccessLayer.EF.Repository";
        private const string DbContextKey = "ED47.GrcTool.Entities.GrcToolContext";
        private const string ImmediateDbContextKey = "ED47.GrcTool.Entities.GrcToolContext.Immediate";

        private static DbContext _dbContext;

        private static DbContext DbContext
        {
            get
            {
                var items = ContextItemCollection.GetItems();

                if (items == null)
                    return _dbContext ?? (_dbContext = BusinessComponent.Kernel.Get<DbContext>());

                var dbContext = items[DbContextKey] as DbContext;
                if (dbContext == null)
                {
                    dbContext = BusinessComponent.Kernel.Get<DbContext>();
                    items.Add(DbContextKey, dbContext);
                }

                return dbContext;
            }
        }

        private static DbContext _immediateDbContext;

        private static DbContext ImmediateDbContext
        {
            get
            {
                var items = ContextItemCollection.GetItems();

                if (items == null)
                    return _immediateDbContext ?? (_immediateDbContext = BusinessComponent.Kernel.Get<DbContext>(
                        new ConstructorArgument("existingConnection", DbContext.Database.Connection),
                        new ConstructorArgument("contextOwnsConnection", false)));

                var dbContext = items[ImmediateDbContextKey] as DbContext;
                if (dbContext == null)
                {
                    dbContext = BusinessComponent.Kernel.Get<DbContext>(
                        new ConstructorArgument("existingConnection", DbContext.Database.Connection),
                        new ConstructorArgument("contextOwnsConnection", false));

                    items.Add(ImmediateDbContextKey, dbContext);
                }

                return dbContext;
            }
        }

        private static Repository _current;

        public static Repository Current
        {
            get
            {
                var items = ContextItemCollection.GetItems();

                if (items == null)
                    return _current ?? (_current = new Repository(DbContext, ImmediateDbContext));

                var repository = items[GenericRepositoryKey] as Repository;
                if (repository == null)
                {
                    repository = new Repository(DbContext, ImmediateDbContext, ContextUsername.Get());
                    items.Add(GenericRepositoryKey, repository);
                }
                return repository;
            }
        }

        public ISqlRepository Repository
        {
            get { return Current; }
        }

        public static IRepository Create()
        {
            var dbContext = BusinessComponent.Kernel.Get<DbContext>();

            return new Repository(dbContext, BusinessComponent.Kernel.Get<DbContext>(new ConstructorArgument("existingConnection", dbContext.Database.Connection), new ConstructorArgument("contextOwnsConnection", false)));
        }

        IRepository BusinessAccessLayer.IGenericRepositoryManager.Repository
        {
            get { return Repository; }
        }
    }
}