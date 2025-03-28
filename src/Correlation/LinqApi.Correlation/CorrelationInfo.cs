namespace LinqApi.Correlation
{
    /// <summary>
    /// Represents immutable correlation metadata that carries trace information such as parent correlation ID and operation step.
    /// Useful for distributed logging, diagnostics, and operation tracking.
    /// </summary>
    public sealed class CorrelationInfo
    {
        /// <summary>
        /// Gets the parent correlation identifier that originated the trace.
        /// </summary>
        public CorrelationId ParentId { get; }

        /// <summary>
        /// Gets the step in the trace chain. The root operation starts at 0 and increments for each child operation.
        /// </summary>
        public int Step { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationInfo"/> class.
        /// </summary>
        /// <param name="parentId">The parent correlation ID.</param>
        /// <param name="step">The current step of the trace. Default is 0.</param>
        public CorrelationInfo(CorrelationId parentId, int step = 0)
        {
            ParentId = parentId;
            Step = step;
        }

        /// <summary>
        /// Creates a new <see cref="CorrelationInfo"/> with incremented step value.
        /// Useful when initiating a child operation.
        /// </summary>
        /// <returns>A new <see cref="CorrelationInfo"/> instance with Step + 1.</returns>
        public CorrelationInfo WithNextStep()
        {
            return new CorrelationInfo(ParentId, Step + 1);
        }

        /// <summary>
        /// Returns a human-readable string representation.
        /// </summary>
        public override string ToString()
        {
            return $"{ParentId}-s{Step}";
        }
    }


}

