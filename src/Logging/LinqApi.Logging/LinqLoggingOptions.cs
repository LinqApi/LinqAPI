namespace LinqApi.Core
{
    /// <summary>
    /// Options for configuring the logging system.
    /// </summary>
    public class LinqLoggingOptions
    {
        /// <summary>
        /// The header name used for the correlation ID. Default is "X-Correlation-Id".
        /// </summary>
        public string CorrelationHeaderName { get; set; }

        /// <summary>
        /// The log category to disable internal logging (to prevent circular logging).
        /// </summary>
        public string InternalLogCategory { get; set; }

        /// <summary>
        /// Flag to enable detailed view logging. If false, HTML view responses are not logged.
        /// </summary>
        public bool LogViewContent { get; set; } = false;

        public string LogLevel { get; set; } = "Information";
    }


}

