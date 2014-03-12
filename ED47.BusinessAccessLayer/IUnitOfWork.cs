using System;

namespace ED47.BusinessAccessLayer
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();
        void Rollback();
        void StartTransaction();
    }
}