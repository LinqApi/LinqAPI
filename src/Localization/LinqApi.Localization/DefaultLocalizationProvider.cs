using System.Collections.Concurrent;

namespace LinqApi.Repository
{
    /// <summary>
    /// The default implementation of <see cref="ILocalizationProvider"/> that first checks an in-memory cache
    /// and then retrieves entries from the localization repository.
    /// </summary>
    public class DefaultLocalizationProvider : ILocalizationProvider
    {
        private readonly ILocalizationRepository _repository;
        private readonly ConcurrentDictionary<string, string> _cache = new();

        public DefaultLocalizationProvider(ILocalizationRepository repository)
        {
            _repository = repository;
        }

        public async Task<string?> GetLocalizedValueAsync(string key, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(key, out string value))
            {
                return value;
            }

            // Load all localization entries (you might want a more efficient lookup in a real implementation)
            var entries = await _repository.GetAllAsync(cancellationToken);
            foreach (var entry in entries)
            {
                // Assume GetLocalizationKeyPrefix() + "Name" for example is the key.
                
                if (!_cache.ContainsKey(key))
                {
                    _cache.TryAdd(key, entry.Value);
                }
            }

            _cache.TryGetValue(key, out value);
            return value;
        }
    }

}
