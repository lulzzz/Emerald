using Emerald.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Emerald.AspNetCore.Transaction
{
    internal sealed class TransactionScopeFactory<TDbContext> : ITransactionScopeFactory where TDbContext : DbContext
    {
        public ITransactionScope Create(IServiceScope serviceScope)
        {
            var dbContext = serviceScope.ServiceProvider.GetService<TDbContext>();
            var transaction = dbContext.Database.BeginTransaction();
            return new TransactionScope(transaction);
        }
    }
}