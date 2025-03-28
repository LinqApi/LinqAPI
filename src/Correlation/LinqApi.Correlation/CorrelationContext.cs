namespace LinqApi.Correlation
{
    /// <summary>
    /// Manages the current correlation context using AsyncLocal.
    /// </summary>
    public static class CorrelationContext
    {
        private static readonly AsyncLocal<CorrelationInfo> _current = new();

        /// <summary>
        /// Gets or sets the current correlation info.
        /// </summary>
        public static CorrelationInfo Current
        {
            get => _current.Value;
            private set => _current.Value = value;
        }

        /// <summary>
        /// Ensures a correlation info is available in the current context.
        /// If none exists, it generates one using the provided generator.
        /// </summary>
        /// <param name="generator">The correlation ID generator.</param>
        public static void EnsureCorrelation(ICorrelationIdGenerator generator)
        {
            if (Current == null)
            {
                var correlationId = generator.Generate(1, 1);
                Current = new CorrelationInfo(correlationId);
            }
        }

        /// <summary>
        /// Creates a new correlation ID with incremented step and updates the current context.
        /// </summary>
        /// <param name="generator">The correlation ID generator.</param>
        /// <returns>The correlation string (e.g., "guid-s1").</returns>
        public static string GetNextCorrelationId(ICorrelationIdGenerator generator)
        {
            EnsureCorrelation(generator);
            Current = Current.WithNextStep(); // Replace with incremented step
            return Current.ToString();
        }

        /// <summary>
        /// Resets the correlation context explicitly (for testing or manual override).
        /// </summary>
        public static void Reset() => _current.Value = null;
    }

}