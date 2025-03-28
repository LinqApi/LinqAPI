using LinqApi.Core.Log;

namespace LinqApi.Logging
{
    /// <summary>
    /// Represents a SQL log entry.
    /// </summary>
    public class LinqSqlLog : LinqLogEntity
    {
        /// <summary>
        /// The SQL command text that was executed.
        /// </summary>
        public string? QueryText { get; set; }

        /// <summary>
        /// Execution duration in milliseconds.
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// The ID of the user who executed the command.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// The date and time when the command was executed.
        /// </summary>
        public DateTime ExecutedAt { get; set; }

        /// <summary>
        /// The type of command (e.g. Reader, NonQuery, Scalar).
        /// </summary>
        public string? CommandType { get; set; }

        public override string LogType => "Database";
    }

}

