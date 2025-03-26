namespace LinqApi.Model
{
    public abstract class BaseLogEntity : BaseEntity<string>
    {
        public string CorrelationId { get; set; }
        public long DurationMs { get; set; }
        public string Exception { get; set; }
        // Internal flag ile loglama sisteminin kendi loglarını ayırabiliriz.
        public bool IsInternal { get; set; }
    }

    public class LinqHttpCallLog : BaseLogEntity
    {
        public string ParentCorrelationId { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public DateTime CreatedAt { get; set; }
        // Ek alanlar: Controller, Action gibi veriler
        public string Controller { get; set; }
        public string Action { get; set; }
        public int StatusCode { get; set; }
    }

    public class LinqEventLog : BaseLogEntity
    {
        public string QueueName { get; set; }
        public string OperationName { get; set; }
        public string RequestPayload { get; set; }
        public string ResponsePayload { get; set; }
        public string MachineName { get; set; }
        public bool Success { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
