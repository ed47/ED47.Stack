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
                throw new ApplicationException("UnitOfWork disposed with pending transaction! Either call Commit() or Rollback() before disposing!");
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
        }
    }
}
