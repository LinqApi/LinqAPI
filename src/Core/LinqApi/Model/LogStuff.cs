using LinqApi.Core;

namespace LinqApi.Model
{
    // Base log model (soyut)
    public abstract class LinqLogEntity : BaseEntity<long>
    {

        protected LinqLogEntity()
        {
            CorrelationId = CorrelationContext.GetNextCorrelationId(new DefaultCorrelationIdGenerator());
        }
        public string CorrelationId { get; private set; }
        public string ParentCorrelationId { get; set; }
        public long DurationMs { get; set; }
        public string Exception { get; set; }
        public bool IsException { get; set; }
        public bool IsInternal { get; set; }
        public DateTime CreatedAt { get; set; }
        public long Epoch { get; set; }

        // Discriminator olarak kullanılacak; her türetilmiş sınıf override eder.
        public virtual string LogType { get; set; }
    }

    // HTTP log
    public class LinqHttpCallLog : LinqLogEntity
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public int StatusCode { get; set; }

        public override string LogType { get; set; } = "HttpCall";
        public string UserAgent { get; set; }
    }

    // Event log (örneğin MassTransit)
    public class LinqEventLog : LinqLogEntity
    {
        public string QueueName { get; set; }
        public string OperationName { get; set; }
        public string RequestPayload { get; set; }
        public string ResponsePayload { get; set; }
        public string MachineName { get; set; }
        public bool Success { get; set; }

        public override string LogType { get; set; } = "Event";
    }

    // Hata logları için soyut model (türetilmiş hata tiplerini destekler)
    public abstract class LinqErrorLog : LinqLogEntity
    {
        public string StackTrace { get; set; }
        // LogType override alt sınıflarda yapılır.
    }

    // Örneğin, Database error log
    public class LinqSqlErrorLog : LinqErrorLog
    {
        public override string LogType { get; set; } = "DatabaseError";
    }

    // Örneğin, Publish error log
    public class LinqPublishErrorLog : LinqErrorLog
    {
        public override string LogType { get; set; } = "PublishError";
    }

    // Örneğin, Consume error log
    public class LinqConsumeErrorLog : LinqErrorLog
    {
        public override string LogType { get; set; } = "ConsumeError";
    }
}