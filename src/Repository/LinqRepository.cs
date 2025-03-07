using LinqApi.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LinqApi.Repository
{
    public class LinqRepository<TDbContext, TEntity, TId> : ILinqRepository<TEntity, TId>
      where TDbContext : DbContext
      where TEntity : class
    {
        protected readonly TDbContext DbContext;
        protected readonly DbSet<TEntity> DbSet;

        public LinqRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
            DbSet = dbContext.Set<TEntity>();
        }

        public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await DbContext.AddAsync(entity, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbContext.Entry(entity).State = EntityState.Modified;
            await SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                DbContext.Entry(entity).State = EntityState.Deleted;
                await SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<TEntity> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        {
            return await DbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await DbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await DbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<PaginationModel<TEntity>> GetPagedFilteredAsync(
            Expression<Func<TEntity, bool>> predicate,
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, object>> orderBy = null,
            bool descending = false,
            CancellationToken cancellationToken = default)
        {
            var query = DbSet.Where(predicate);


            if (orderBy != null)
            {
                query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken);

            return new PaginationModel<TEntity> { Items = items, TotalRecords = totalCount };
        }
    }


}
