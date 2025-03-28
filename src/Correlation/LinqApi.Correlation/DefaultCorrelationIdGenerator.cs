
namespace LinqApi.Correlation
{
    /// <summary>
    /// Provides a default implementation of the <see cref="ICorrelationIdGenerator"/> interface.
    /// This generator creates a <see cref="CorrelationId"/> embedding environment and source metadata.
    /// </summary>
    public sealed class DefaultCorrelationIdGenerator : ICorrelationIdGenerator
    {
        /// <summary>
        /// Creates a new <see cref="CorrelationId"/> using the specified environment and source type codes.
        /// </summary>
        /// <param name="environment">
        /// A byte representing the environment code (e.g., 1 = Production, 2 = Staging, etc.).
        /// </param>
        /// <param name="sourceType">
        /// A byte representing the source type (e.g., 1 = Web, 2 = Worker, 3 = External API).
        /// </param>
        /// <returns>
        /// A new instance of <see cref="CorrelationId"/> encoded with the specified metadata and current epoch.
        /// </returns>
        public CorrelationId Generate(byte environment, byte sourceType)
        {
            return CorrelationId.Create(environment, sourceType);
        }
    }

}

