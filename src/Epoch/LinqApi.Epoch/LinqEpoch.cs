using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace LinqApi.Epoch
{
    public static class LinqEpoch
    {
        // Temel epoch tarihi: 1 Ocak 2024 (UTC)
        private static readonly DateTime _baseEpoch = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Epoch periyodu: 3 dakika (180 saniye)
        private const int SecondsPerEpoch = 180;

        // In-Memory Cache instance (singleton düzeyinde)
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        private const string EpochCacheKey = "CurrentEpoch";

        /// <summary>
        /// Şu anki epoch değerini döner. Değer, her 3 dakikada bir güncellenir.
        /// </summary>
        public static long Now()
        {
            if (!_cache.TryGetValue(EpochCacheKey, out long epoch))
            {
                // Yeni epoch değeri hesapla
                double totalSeconds = (DateTime.Now - _baseEpoch).TotalSeconds;
                epoch = (long)(totalSeconds / SecondsPerEpoch);

                // Şu anki periyodun bitimine kadar cache'de saklayalım
                // Örneğin, kalan saniyeyi hesaplayıp TTL olarak belirleyelim:
                double secondsPassedInPeriod = totalSeconds % SecondsPerEpoch;
                var ttl = TimeSpan.FromSeconds(SecondsPerEpoch - secondsPassedInPeriod);

                _cache.Set(EpochCacheKey, epoch, ttl);
            }
            return epoch;
        }
    }

    public static class LinqEpochServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultLinqEpochProvider(this IServiceCollection services)
        {
            services.AddSingleton<IEpochProvider, DefaultLinqEpochProvider>();
            return services;
        }
    }
}
