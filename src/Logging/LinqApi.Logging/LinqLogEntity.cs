using LinqApi.Correlation;

namespace LinqApi.Core.Log
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
}

