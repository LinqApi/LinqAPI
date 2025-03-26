namespace LinqApi.Core
{
    /// <summary>
    /// Manages the current correlation context using AsyncLocal.
    /// </summary>
    public static class CorrelationContext
    {
        private static readonly AsyncLocal<CorrelationInfo> _current = new();
        public static CorrelationInfo Current
        {
            get => _current.Value;
            set => _current.Value = value;
        }

        /// <summary>
        /// Ensures that a correlation context exists; if not, creates one.
        /// </summary>
        public static void EnsureCorrelation(ICorrelationIdGenerator generator)
        {
            if (Current == null)
            {
                Current = new CorrelationInfo
                {
                    ParentId = generator.Generate(1, 1),
                    Step = 0
                };
            }
        }

        /// <summary>
        /// Increments the step counter and returns a new correlation string.
        /// </summary>
        public static string GetNextCorrelationId(ICorrelationIdGenerator generator)
        {
            EnsureCorrelation(generator);
            Current.Step++;
            return $"{Current.ParentId}-{Current.Step}";
        }
    }


}

