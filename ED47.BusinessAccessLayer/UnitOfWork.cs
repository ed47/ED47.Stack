using System;
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
            StartTransaction();
        }

        public UnitOfWork()
        {
            Repository = BaseUserContext.Instance.Repository;
            StartTransaction();
            Repository.Transaction = Transaction;
        }

        private void StartTransaction()
        {
            Transaction = Repository.DbContext.Database.BeginTransaction();
            Repository.ImmediateDbContext.Database.UseTransaction(Transaction.UnderlyingTransaction);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
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
            {
                Transaction.Commit();
                // Transaction.Commit() has set Repository.DbContext.Database.Transaction to null
                // but not Repository.ImmediateDbContext.Database.Transaction. this will fix it :
                Repository.ImmediateDbContext.Database.UseTransaction(null);
                Repository.Transaction = null;
                Transaction = null;
            }
        }

        public void Rollback()
        {
            if (Repository == null)
                return;

            if (Transaction == null)
                return;

            Transaction.Rollback();
            Transaction.Dispose();
            Transaction = null;
            Repository.ImmediateDbContext.Database.UseTransaction(null);
            Repository.Transaction = null;
        }
    }
}
