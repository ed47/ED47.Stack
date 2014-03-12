using System;
using System.Data.Entity;
using Ninject;

namespace ED47.BusinessAccessLayer.EF
{
    public class UnitOfWork<TContextType> : IUnitOfWork where TContextType : DbContext
    {
        private TContextType _immediateDbContext;
        private DbContextTransaction Transaction { get; set; }
        public TContextType DbContext { get; private set; }

        public TContextType ImmediateDbContext
        {
            get
            {
                return _immediateDbContext ?? (_immediateDbContext = (TContextType)Activator.CreateInstance(typeof(TContextType), new object[] { DbContext.Database.Connection, false }));
            }
        }

        [Inject]
        public UnitOfWork(TContextType dbContext)
        {
            DbContext = dbContext;
        }

        public void StartTransaction()
        {
            Transaction = DbContext.Database.BeginTransaction();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (DbContext != null)
                DbContext.Dispose();

            if (Transaction != null)
            {
                Transaction.Rollback();
                Transaction.Dispose();
            }
        }

        public void Commit()
        {
            if (DbContext == null)
                return;

            DbContext.SaveChanges();

            if (Transaction != null)
                Transaction.Commit();
        }

        public void Rollback()
        {
            if (DbContext == null)
                return;

            if (Transaction == null)
                return;

            Transaction.Rollback();
            Transaction.Dispose();
        }
    }
}
