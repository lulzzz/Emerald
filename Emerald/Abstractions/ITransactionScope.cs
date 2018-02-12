using System;

namespace Emerald.Abstractions
{
    public interface ITransactionScope : IDisposable
    {
        void Commit();
        void Rollback();
    }
}