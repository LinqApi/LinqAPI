using LinqApi.Model;

namespace LinqApi.Core
{
    /// <summary>
    /// Base log entity containing common fields.
    /// </summary>
    public abstract class BaseLogEntity : BaseEntity<string>
    {
        public string CorrelationId { get; set; }
        public long DurationMs { get; set; }
        public string Exception { get; set; }
        public bool IsException { get; set; }
        public bool IsInternal { get; set; }
        // Optionally, you can add a field for log level, source, etc.
        public DateTime CreatedAt { get; set; }

        public LogType LogType { get; set; }
    }


}

