using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emerald.AspNetCore.EntityFrameworkCore
{
    public abstract class Repository<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;

        protected Repository(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(TEntity entity)
        {
            _dbContext.Set<TEntity>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await LoadReferencesAndCollections(entity);
        }
        public async Task AddRange(IEnumerable<TEntity> entities)
        {
            var entityArray = entities.ToArray();
            _dbContext.Set<TEntity>().AddRange(entityArray);
            await _dbContext.SaveChangesAsync();
            foreach (var entity in entityArray) await LoadReferencesAndCollections(entity);
        }

        public async Task Update(TEntity entity)
        {
            _dbContext.Set<TEntity>().Update(entity);
            await _dbContext.SaveChangesAsync();
            await LoadReferencesAndCollections(entity);
        }
        public async Task UpdateRange(IEnumerable<TEntity> entities)
        {
            var entityArray = entities.ToArray();
            _dbContext.Set<TEntity>().UpdateRange(entityArray);
            await _dbContext.SaveChangesAsync();
            foreach (var entity in entityArray) await LoadReferencesAndCollections(entity);
        }

        public async Task Remove(TEntity entity)
        {
            _dbContext.Set<TEntity>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
        public async Task RemoveRange(IEnumerable<TEntity> entities)
        {
            _dbContext.Set<TEntity>().RemoveRange(entities);
            await _dbContext.SaveChangesAsync();
        }

        private async Task LoadReferencesAndCollections(object entity)
        {
            foreach (var collection in _dbContext.Entry(entity).Collections)
            {
                await collection.LoadAsync();
                foreach (var collectionItem in collection.CurrentValue) await LoadReferencesAndCollections(collectionItem);
            }

            foreach (var reference in _dbContext.Entry(entity).References)
            {
                await reference.LoadAsync();
                await LoadReferencesAndCollections(reference.CurrentValue);
            }
        }
    }
}