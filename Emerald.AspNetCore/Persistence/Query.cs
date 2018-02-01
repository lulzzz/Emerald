using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Emerald.AspNetCore.Persistence
{
    public abstract class Query<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;

        protected Query(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected TDbContext DbContext => _dbContext;

        public IQueryable<TEntity> Source => _dbContext.Set<TEntity>();
    }
}