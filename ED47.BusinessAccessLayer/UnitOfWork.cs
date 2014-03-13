using System.Data.Entity;

namespace ED47.BusinessAccessLayer
{
    public class UnitOfWork : IUnitOfWork
    {
        private DbContextTransaction Transaction { get; set; }
        public IRepository Repository { get; private set; }

        public UnitOfWork(IRepository repository)
        {
            Repository = repository;
        }

        public void StartTransaction()
        {
            Transaction = Repository.DbContext.Database.BeginTransaction();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Repository != null)
                Repository.Dispose();

            if (Transaction != null)
            {
                Transaction.Rollback();
                Transaction.Dispose();
            }
        }

        public void Commit()
        {
            if (Repository == null)
                return;

            Repository.Commit();

            if (Transaction != null)
                Transaction.Commit();
        }

        public void Rollback()
        {
            if (Repository == null)
                return;

            if (Transaction == null)
                return;

            Transaction.Rollback();
            Transaction.Dispose();
        }
    }
}
