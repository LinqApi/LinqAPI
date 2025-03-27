namespace LinqApi.Epoch
{
    /// <summary>
    /// epoch projesine gidecek =>  o da LinqApi.Epoch diye bir nuget olacak.
    /// </summary>
    public class DefaultLinqEpochProvider : IEpochProvider
    {
        // Temel epoch tarihi: 1 Ocak 2024 (UTC)
        private readonly DateTime _baseEpoch = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        // Her epoch birimi 3 dakika (180 saniye)
        private const int SecondsPerEpoch = 180;

        public long GetEpoch(DateTime createdAt)
        {
            // Oluşturulma tarihini UTC’ye çeviriyoruz.
            DateTime utcCreatedAt = createdAt.ToUniversalTime();
            double totalSeconds = (utcCreatedAt - _baseEpoch).TotalSeconds;
            return (long)(totalSeconds / SecondsPerEpoch);
        }
    }
}
