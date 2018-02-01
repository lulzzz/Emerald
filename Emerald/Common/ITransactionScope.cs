using System;

namespace Emerald.Common
{
    public interface ITransactionScope : IDisposable
    {
        void Commit();
        void Rollback();
    }
}