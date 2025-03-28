using LinqApi.Core;
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
        where TEntity : class
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
    }

}
