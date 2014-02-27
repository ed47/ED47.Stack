using System;
using System.Data.Entity;

namespace ED47.BusinessAccessLayer
{
    public interface IUnitOfWork : IDisposable
    {
        DbContext DbContext { get; }
        DbContext ImmediateDbContext { get; }

        void Commit();
    }
}