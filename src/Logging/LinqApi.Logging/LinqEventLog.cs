using LinqApi.Logging.Log;

namespace LinqApi.Logging
{

    /// <summary>
    /// Represents a structured log entry for event-based operations, such as publish or consume events
    /// within a message-driven architecture.
    /// </summary>
    /// <remarks>
    /// This log entity is useful for tracking request/response payloads, queue information,
    /// operation context, machine details, and success status. It is commonly used in
    /// MassTransit, RabbitMQ, AWS SQS/SNS or similar messaging infrastructures.
    /// </remarks>
    public class LinqEventLog : LinqLogEntity
    {
        /// <summary>
        /// Gets or sets the name of the queue associated with the event.
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// Gets or sets the name of the operation performed (e.g., Publish-OrderCreated).
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        /// Gets or sets the payload sent during the request phase of the event.
        /// </summary>
        public string RequestPayload { get; set; }

        /// <summary>
        /// Gets or sets the payload or message returned in response to the event.
        /// </summary>
        public string ResponsePayload { get; set; }

        /// <summary>
        /// Gets or sets the name of the machine that processed the event.
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the event operation was successful.
        /// </summary>
        public bool Success { get; set; }

    }
}

