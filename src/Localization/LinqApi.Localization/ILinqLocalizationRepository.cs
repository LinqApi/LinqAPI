using LinqApi.Localization.LinqApi.Localization;

namespace LinqApi.Repository
{
    /// <summary>
    /// Provides methods for retrieving and managing localization entries from the database.
    /// </summary>
    public interface ILocalizationRepository
    {
        /// <summary>
        /// Retrieves all localization entries from the data store.
        /// </summary>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A collection of localization entities.</returns>
        Task<IEnumerable<LinqLocalizationEntity>> GetAllAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Inserts or updates the given localization entry.
        /// </summary>
        /// <param name="entity">The localization entity to add or update.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>The upserted localization entity.</returns>
        Task<LinqLocalizationEntity> UpsertAsync(LinqLocalizationEntity entity, CancellationToken cancellationToken);
    }

}
