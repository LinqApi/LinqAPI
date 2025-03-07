using LinqApi.Model;
using System.Linq.Expressions;

namespace LinqApi.Repository
{
    public interface ILinqRepository<TEntity, TId>
     where TEntity : class
    {
        Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
        Task<TEntity> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<PaginationModel<dynamic>> GetFilterPagedAsync(
      LinqFilterModel filterModel,
      CancellationToken cancellationToken = default);
    }

}
