namespace LinqApi.Localization
{
    using global::LinqApi.Repository;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    namespace LinqApi.Localization.Extensions
    {
        /// <summary>
        /// A hosted service that seeds localization data from the database into the in-memory cache
        /// during application startup.
        /// </summary>
        public class LocalizationSeederHostedService : IHostedService
        {
            private readonly ILocalizationRepository _repository;
            private readonly ILocalizationProvider _provider;
            private readonly ILogger<LocalizationSeederHostedService> _logger;

            public LocalizationSeederHostedService(
                ILocalizationRepository repository,
                ILocalizationProvider provider,
                ILogger<LocalizationSeederHostedService> logger)
            {
                _repository = repository;
                _provider = provider;
                _logger = logger;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("Seeding localization data...");
                // Load all localization entries to populate the provider cache.
                var entries = await _repository.GetAllAsync(cancellationToken);
                foreach (var entry in entries)
                {
                    await _provider.GetLocalizedValueAsync(entry.Key, cancellationToken);
                }
                _logger.LogInformation("Localization seeding completed.");
            }

            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }
    }
}