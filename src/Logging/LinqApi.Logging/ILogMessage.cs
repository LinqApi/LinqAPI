namespace LinqApi.Logging
{
    /// <summary>
    /// A simple log message contract for MassTransit.
    /// </summary>
    public interface ILogMessage
    {
        string? CorrelationId { get; }
        string? ParentCorrelationId { get; }
        /// <summary>
        /// A discriminator indicating the log type (e.g. "ApiLog", "HttpCallLog", "DbLog").
        /// </summary>
        string? MessageType { get; }
        /// <summary>
        /// The payload of the log (can be JSON, XML, etc.).
        /// </summary>
        string? Payload { get; }
        DateTime CreatedAt { get; }
    }


}

