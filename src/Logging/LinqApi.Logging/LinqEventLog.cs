using LinqApi.Core.Log;

namespace LinqApi.Logging
{
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
}

