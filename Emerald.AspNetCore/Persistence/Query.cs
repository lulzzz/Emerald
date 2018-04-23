using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Emerald.AspNetCore.Persistence
{
    public abstract class Query<TDbContext> where TDbContext : DbContext
    {
        protected Query(TDbContext dbContext)
        {
            DbContext = dbContext;
        }

        protected TDbContext DbContext { get; }
    }

    public abstract class Query<TEntity, TDbContext> : Query<TDbContext> where TDbContext : DbContext where TEntity : class
    {
        protected Query(TDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<TEntity> Source => ConfigureSource(DbContext.Set<TEntity>());

        protected virtual IQueryable<TEntity> ConfigureSource(IQueryable<TEntity> source)
        {
            return source;
        }
    }
}