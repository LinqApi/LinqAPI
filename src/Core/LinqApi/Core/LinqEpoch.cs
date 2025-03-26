namespace LinqApi.Core
{
    /// <summary>
    /// Helper class for epoch calculations.
    /// </summary>
    public static class LinqEpoch
    {
        public const long EpochLengthMs = 240_000; // 4 minutes

        public static long GetEpoch(DateTimeOffset dateTime)
            => dateTime.ToUnixTimeMilliseconds() / EpochLengthMs;

        public static DateTimeOffset GetDateTime(long epoch)
            => DateTimeOffset.FromUnixTimeMilliseconds(epoch * EpochLengthMs);

        public static long Now()
            => GetEpoch(DateTimeOffset.UtcNow);
    }


}

