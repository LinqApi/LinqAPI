namespace LinqApi.Core
{
    /// <summary>
    /// Event log entity.
    /// </summary>
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

