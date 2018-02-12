using Emerald.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Emerald.AspNetCore.Infrastructure
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
            _transaction.Rollback();
        }
    }
}