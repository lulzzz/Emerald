using System;

namespace Emerald.System
{
    public interface ITransactionScope : IDisposable
    {
        void Commit();
        void Rollback();
    }
}