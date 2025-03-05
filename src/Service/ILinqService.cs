using LinqApi.Model;
using System.Linq.Expressions;

namespace LinqApi.Service
{
    public interface ILinqService<T1, T2, TId>
    where T1 : class
    where T2 : class
    {
        Task<T2> InsertAsync(T2 dto, CancellationToken cancellationToken = default);
        Task<T2> UpdateAsync(T2 dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
        Task<T2> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T2>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<T2>> FindAsync(Expression<Func<T1, bool>> predicate, CancellationToken cancellationToken = default);
        Task<PaginationModel<T2>> GetPagedFilteredAsync(
             Expression<Func<T1, bool>> predicate,
             int pageNumber,
             int pageSize,
             List<string> includes = null,
             Expression<Func<T1, object>> orderBy = null,
             bool descending = false,
             CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

}
