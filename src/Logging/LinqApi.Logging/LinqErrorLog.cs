using LinqApi.Core.Log;

namespace LinqApi.Core
{

    /// <summary>
    /// Represents a base class for logging error-related events within the LINQ-based logging infrastructure.
    /// </summary>
    /// <remarks>
    /// This class is intended to be inherited by more specific error log types such as 
    /// <see cref="LinqConsumeErrorLog"/> or <see cref="LinqPublishErrorLog"/>. It extends
    /// <see cref="LinqLogEntity"/> with additional error-specific information.
    /// </remarks>
    public abstract class LinqErrorLog : LinqLogEntity
    {
        /// <summary>
        /// Gets or sets the stack trace associated with the error.
        /// </summary>
        /// <value>A string representation of the exception's stack trace.</value>
        public string StackTrace { get; set; }
    }
}

