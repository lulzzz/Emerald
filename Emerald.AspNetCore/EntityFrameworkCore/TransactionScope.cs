using Emerald.System;
using Microsoft.EntityFrameworkCore.Storage;
using System;

namespace Emerald.AspNetCore.EntityFrameworkCore
{
    internal sealed class TransactionScope : ITransactionScope
    {
        private readonly IDbContextTransaction _transaction;

        public TransactionScope(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public void Dispose()
        {
            _transaction.Dispose();
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            try
            {
                _transaction.Rollback();
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}