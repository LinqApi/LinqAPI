namespace LinqApi.Core
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
    }
}

