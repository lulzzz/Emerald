using Emerald.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Emerald.AspNetCore.EntityFrameworkCore
{
    internal sealed class TransactionScopeFactory<TDbContext> : ITransactionScopeFactory where TDbContext : DbContext
    {
        public ITransactionScope Create(Emerald.System.IServiceScope serviceScope)
        {
            var dbContext = serviceScope.ServiceProvider.GetService<TDbContext>();
            var transaction = dbContext.Database.BeginTransaction();
            return new TransactionScope(transaction);
        }
    }
}