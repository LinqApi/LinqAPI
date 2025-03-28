namespace LinqApi.Correlation
{
    /// <summary>
    /// Defines a contract for generating correlation identifiers used to trace and track operations across systems.
    /// </summary>
    public interface ICorrelationIdGenerator
    {
        /// <summary>
        /// Generates a new <see cref="CorrelationId"/> using the specified metadata.
        /// </summary>
        /// <param name="environment">
        /// A byte value that indicates the environment (e.g., 1 = Production, 2 = Staging, 3 = Development).
        /// </param>
        /// <param name="sourceType">
        /// A byte value that represents the origin of the request (e.g., 1 = Web, 2 = BackgroundJob, 3 = External).
        /// </param>
        /// <returns>
        /// A new instance of <see cref="CorrelationId"/> that embeds the environment and source type codes.
        /// </returns>
        CorrelationId Generate(byte environment, byte sourceType);
    }

}

