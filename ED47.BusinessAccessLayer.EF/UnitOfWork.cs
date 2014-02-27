using System.Data.Entity;
using Ninject;

namespace ED47.BusinessAccessLayer.EF
{
    public class UnitOfWork : IUnitOfWork
    {
        public DbContext DbContext { get; private set; }
        public DbContext ImmediateDbContext { get; private set; }

        [Inject]
        public UnitOfWork(DbContext dbContext, DbContext immediateDbContext)
        {
            DbContext = dbContext;
            ImmediateDbContext = immediateDbContext;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if(DbContext != null)
                DbContext.Dispose();
        }

        public void Commit()
        {
            if (DbContext == null)
                return;
            
            DbContext.SaveChanges();
        }
    }
}
