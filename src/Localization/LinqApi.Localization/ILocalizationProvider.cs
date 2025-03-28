namespace LinqApi.Repository
{
    /// <summary>
    /// Provides methods for retrieving localized strings based on keys.
    /// </summary>
    public interface ILocalizationProvider
    {
        /// <summary>
        /// Retrieves the localized string corresponding to the given key.
        /// </summary>
        /// <param name="key">The localization key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The localized string, or null if not found.</returns>
        Task<string?> GetLocalizedValueAsync(string key, CancellationToken cancellationToken);
    }

}
