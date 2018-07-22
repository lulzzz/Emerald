using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Emerald.AspNetCore.EntityFrameworkCore
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

        public IQueryable<TEntity> Set => DbContext.Set<TEntity>();
        public IQueryable<TEntity> Source => ConfigureSource(Set);

        protected virtual IQueryable<TEntity> ConfigureSource(IQueryable<TEntity> source)
        {
            return source;
        }
    }
}