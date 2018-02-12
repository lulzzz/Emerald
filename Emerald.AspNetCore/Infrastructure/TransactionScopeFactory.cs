using Emerald.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Emerald.AspNetCore.Infrastructure
{
    internal sealed class TransactionScopeFactory<TDbContext> : ITransactionScopeFactory where TDbContext : DbContext
    {
        public ITransactionScope Create(Abstractions.IServiceScope serviceScope)
        {
            var dbContext = serviceScope.ServiceProvider.GetService<TDbContext>();
            var transaction = dbContext.Database.BeginTransaction();
            return new TransactionScope(transaction);
        }
    }
}