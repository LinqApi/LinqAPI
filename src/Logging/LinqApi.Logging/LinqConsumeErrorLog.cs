namespace LinqApi.Logging
{
    /// <summary>
    /// Represents a log entry for errors that occur during the consumption of a message from a queue.
    /// </summary>
    /// <remarks>
    /// This class is a specialized form of <see cref="LinqErrorLog"/> that is used specifically
    /// to capture and identify consume-related faults in message processing.
    /// </remarks>
    public class LinqConsumeErrorLog : LinqErrorLog
    {
        /// <summary>
        /// Gets or sets the type of the log entry. Default is "ConsumeError".
        /// </summary>
        /// <value>A string that identifies this log as a consume error.</value>
        public override string LogType { get; set; } = "ConsumeError";
    }
}

