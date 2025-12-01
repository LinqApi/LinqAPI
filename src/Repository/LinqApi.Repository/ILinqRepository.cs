using LinqApi.Core;
using LinqApi.Core;
using System.Linq.Expressions;

namespace LinqApi.Repository
{
    /// <summary>
    /// Defines a generic repository interface for querying and managing entities.
    /// This abstraction enables LINQ-based queries, dynamic filtering, pagination, and inclusion of related entities.
    /// It is intended for integration with controller actions or LLM-powered APIs that require flexible querying.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The type of the entity's primary key.</typeparam>
    public interface ILinqRepository<TEntity, TId> where TEntity : BaseEntity<TId>
    {
        Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
        Task<int> DeleteWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<TEntity?> GetByIdAsync(
     TId id,
     IEnumerable<Func<IQueryable<TEntity>, IQueryable<TEntity>>>? includeFunctions = null,
     CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds entities matching the predicate. Use when no includes are required.
        /// </summary>
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a single entity matching the predicate with optional eager-loaded navigation properties.
        /// </summary>
        Task<TEntity?> FindWithFilterAsync(Expression<Func<TEntity, bool>> predicate,
            IEnumerable<Func<IQueryable<TEntity>, IQueryable<TEntity>>> includeFunctions,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds multiple entities matching the predicate with optional eager-loaded navigation properties.
        /// </summary>
        Task<IEnumerable<TEntity>> FindManyWithFilterAsync(Expression<Func<TEntity, bool>> predicate,
            IEnumerable<Func<IQueryable<TEntity>, IQueryable<TEntity>>> includeFunctions,
            CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to find an entity by querying for its Id first, then retrieving the full object.
        /// Useful for lightweight optimization.
        /// </summary>
        Task<(bool found, TEntity? entity)> TryFindFastAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Applies dynamic filtering, projection, ordering, grouping, and pagination
        /// through a model that supports string-based queries.
        /// Designed to power advanced client-side or AI-driven query composition.
        /// </summary>
        Task<PaginationModel<dynamic>> GetFilterPagedAsync(LinqFilterModel filterModel, CancellationToken cancellationToken = default);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sorgu yazabilmek için IQueryable olarak sunar.
        /// </summary>
        IQueryable<TEntity> Query();
    }

}
