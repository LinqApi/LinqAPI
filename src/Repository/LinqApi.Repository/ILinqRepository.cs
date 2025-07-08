using LinqApi.Logging;
using System.Linq.Expressions;

namespace LinqApi.Repository
{
    /// <summary>
    /// Defines a contract for a generic repository providing asynchronous data access operations
    /// for entities, including basic CRUD operations and dynamic filtering with paging.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TId">The type of the entity identifier.</typeparam>
    public interface ILinqRepository<TEntity, TId>
    where TEntity : BaseEntity<TId>
    {
        /// <summary>
        /// Inserts the specified entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>The inserted entity.</returns>
        Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the specified entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>The updated entity.</returns>
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the entity with the specified identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the entity to delete.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        Task DeleteAsync(TId id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the entity with the specified identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the entity to retrieve.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// The entity with the specified identifier, or <c>null</c> if no matching entity is found.
        /// </returns>
        Task<TEntity> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all entities asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>An enumerable collection of all entities.</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all entities asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>An enumerable collection of all entities.</returns>
        Task<IEnumerable<TEntity>> GetAllWithIncludesAndFilterAsync(Expression<Func<TEntity, bool>> predicate,
    Expression<Func<TEntity, object>>[] includes,
    CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds entities based on the specified predicate asynchronously.
        /// </summary>
        /// <param name="predicate">An expression to filter entities.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>An enumerable collection of entities that match the predicate.</returns>
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists all changes made in the repository to the underlying data store asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>The number of state entries written to the data store.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a paged list of entities based on dynamic filtering criteria.
        /// </summary>
        /// <param name="filterModel">
        /// A model containing filtering, ordering, grouping, and paging parameters.
        /// </param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// A <see cref="PaginationModel{T}"/> object containing the dynamic result set and total record count.
        /// </returns>
        Task<PaginationModel<dynamic>> GetFilterPagedAsync(LinqFilterModel filterModel, CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines whether any entity exists that satisfies the specified predicate.
        /// </summary>
        /// <param name="predicate">An expression to test each entity for a condition.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns><c>true</c> if any entity matches the predicate; otherwise, <c>false</c>.</returns>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tries to find an entity matching the given predicate in two steps:
        /// (1) It selects only the entity's identifier to perform a lightweight check,
        /// (2) If an ID is found, it loads the full entity using that ID.
        /// </summary>
        /// <param name="predicate">An expression used to locate a potential match.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// A tuple where <c>found</c> indicates if an entity was found, and <c>entity</c> contains the entity if found.
        /// </returns>
        Task<(bool found, TEntity? entity)> TryFindFastAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<TEntity?> FindWithIncludesAsync(
    Expression<Func<TEntity, bool>> predicate,
    Expression<Func<TEntity, object>>[] includes,
    CancellationToken cancellationToken = default);

        Task<TEntity?> FindWithFilterAsync(
      Expression<Func<TEntity, bool>> predicate,
      IEnumerable<Func<IQueryable<TEntity>, IQueryable<TEntity>>> includeFunctions,
      CancellationToken cancellationToken = default);

        Task<int> DeleteWhereAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);
    }
}
