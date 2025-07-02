using LinqApi.Logging.Log;
using LinqApi.Correlation;
using LinqApi.Logging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;
using MassTransit.AmazonSqsTransport;

namespace LinqApi.MassTransit
{
    /// <summary>
    /// Observes publishing and consuming events for messages and logs them accordingly.
    /// Implements both <see cref="IPublishObserver"/> and <see cref="IConsumeObserver"/>.
    /// </summary>
    public class LinqLoggingObserver : IPublishObserver, IConsumeObserver
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICorrelationIdGenerator _correlationGenerator;
        private readonly IOptions<LinqLoggingOptions> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinqLoggingObserver"/> class.
        /// </summary>
        /// <param name="scopeFactory">
        /// The service scope factory used to create scopes for resolving scoped services.
        /// </param>
        /// <param name="correlationGenerator">
        /// The correlation ID generator used to ensure messages are correlated.
        /// </param>
        /// <param name="options">
        /// The logging options containing configuration values such as the correlation header name.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any of the required dependencies is <c>null</c>.
        /// </exception>
        public LinqLoggingObserver(
            IServiceScopeFactory scopeFactory,
            ICorrelationIdGenerator correlationGenerator,
            IOptions<LinqLoggingOptions> options)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _correlationGenerator = correlationGenerator ?? throw new ArgumentNullException(nameof(correlationGenerator));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Logs the specified log entry asynchronously by creating a new service scope and resolving the logger.
        /// </summary>
        /// <param name="logEntry">The log entry to log.</param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A task that represents the asynchronous logging operation.</returns>
        private async Task LogAsync(LinqLogEntity logEntry, CancellationToken cancellationToken = default)
        {
            // Create a new scope so that ILinqLogger is resolved as a scoped service.
            using var scope = _scopeFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILinqLogger>();
            await logger.LogAsync(logEntry, cancellationToken).ConfigureAwait(false);
        }

        #region IPublishObserver Members

        /// <summary>
        /// Called before a message is published.
        /// Ensures that a correlation header exists and logs a pre-publish event.
        /// </summary>
        /// <typeparam name="T">The type of the message being published.</typeparam>
        /// <param name="context">The publish context containing message and header information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task PrePublish<T>(PublishContext<T> context) where T : class
        {
            string correlationHeader = _options.Value.CorrelationHeaderName;

            if (context is AmazonSqsMessageSendContext<ReceiveFault>)
            {
                var a = context as AmazonSqsMessageSendContext<ReceiveFault>;

                var address = a.FaultAddress;
            }
            //MassTransit.AmazonSqsTransport.<MassTransit.ReceiveFault>

            if (!context.Headers.TryGetHeader(correlationHeader, out var existing))
            {
                // Ensure a correlation exists; if not, generate one.
                CorrelationContext.EnsureCorrelation(_correlationGenerator);
                var headerCorrelation = CorrelationContext.Current?.ParentId.ToString()
                                        ?? _correlationGenerator.Generate(1, 1).ToString();
                context.Headers.Set(correlationHeader, headerCorrelation);
            }

            var preLog = new LinqEventLog
            {
                OperationName = $"Publish-{typeof(T).Name}",
                RequestPayload = SerializeMessage(context.Message),
                ResponsePayload = string.Empty,
                QueueName = context.DestinationAddress?.ToString() ?? "Unknown",
                Success = false,
                CreatedAt = DateTime.UtcNow
            };

            await LogAsync(preLog).ConfigureAwait(false);
        }

        /// <summary>
        /// Called after a message is successfully published.
        /// Logs a successful publish event.
        /// </summary>
        /// <typeparam name="T">The type of the message that was published.</typeparam>
        /// <param name="context">The publish context containing message and header information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task PostPublish<T>(PublishContext<T> context) where T : class
        {
            var successLog = new LinqEventLog
            {
                OperationName = $"Publish-{typeof(T).Name}",
                RequestPayload = SerializeMessage(context.Message),
                ResponsePayload = "Message published successfully",
                QueueName = context.DestinationAddress?.ToString() ?? "Unknown",
                Success = true,
                CreatedAt = DateTime.UtcNow
            };

            await LogAsync(successLog).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when a publish operation fails.
        /// Logs the publish error event along with exception details.
        /// </summary>
        /// <typeparam name="T">The type of the message being published.</typeparam>
        /// <param name="context">The publish context containing message and header information.</param>
        /// <param name="exception">The exception that occurred during publishing.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
            var faultLog = new LinqPublishErrorLog
            {
                IsException = true,
                CreatedAt = DateTime.UtcNow,
                StackTrace = exception.StackTrace


            };

            await LogAsync(faultLog).ConfigureAwait(false);
        }

        #endregion

        #region IConsumeObserver Members

        /// <summary>
        /// Called before a message is consumed.
        /// Ensures correlation and logs a pre-consume event.
        /// </summary>
        /// <typeparam name="T">The type of the message being consumed.</typeparam>
        /// <param name="context">The consume context containing message and header information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
            string correlationHeader = _options.Value.CorrelationHeaderName;
            if (!context.Headers.TryGetHeader(correlationHeader, out var existing))
            {
                // Ensure a correlation exists; if not, generate one.
                CorrelationContext.EnsureCorrelation(_correlationGenerator);
                var headerCorrelation = CorrelationContext.Current?.ParentId.ToString()
                                        ?? _correlationGenerator.Generate(1, 1).ToString();
                // Note: Header is not set in consume context in this example.
            }

            var preLog = new LinqEventLog
            {
                OperationName = $"Consume-{typeof(T).Name}",
                RequestPayload = SerializeMessage(context.Message),
                ResponsePayload = string.Empty,
                QueueName = context.ReceiveContext.InputAddress?.ToString() ?? "Unknown",
                Success = false,
                CreatedAt = DateTime.UtcNow
            };

            await LogAsync(preLog).ConfigureAwait(false);
        }

        /// <summary>
        /// Called after a message is successfully consumed.
        /// Logs a successful consume event.
        /// </summary>
        /// <typeparam name="T">The type of the message that was consumed.</typeparam>
        /// <param name="context">The consume context containing message and header information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task PostConsume<T>(ConsumeContext<T> context) where T : class
        {
            var successLog = new LinqEventLog
            {
                OperationName = $"Consume-{typeof(T).Name}",
                RequestPayload = SerializeMessage(context.Message),
                ResponsePayload = "Message consumed successfully",
                QueueName = context.ReceiveContext.InputAddress?.ToString() ?? "Unknown",
                Success = true,
                CreatedAt = DateTime.UtcNow
            };

            await LogAsync(successLog).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when a consume operation fails.
        /// Logs the consume error event along with exception details.
        /// </summary>
        /// <typeparam name="T">The type of the message being consumed.</typeparam>
        /// <param name="context">The consume context containing message and header information.</param>
        /// <param name="exception">The exception that occurred during consumption.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
        {
            var faultLog = new LinqConsumeErrorLog
            {
                IsException = true,
                CreatedAt = DateTime.UtcNow,
                StackTrace = exception.StackTrace
            };

            await LogAsync(faultLog, context.CancellationToken).ConfigureAwait(false);
        }

        #endregion

        /// <summary>
        /// Probes the observer to inspect its configuration.
        /// </summary>
        /// <param name="context">The probe context used to record information about this observer.</param>
        public void Probe(ProbeContext context) => context.CreateScope("LinqLoggingObserver");

        /// <summary>
        /// Serializes the given message to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the message to serialize.</typeparam>
        /// <param name="message">The message to serialize.</param>
        /// <returns>
        /// A JSON string representation of the message; if serialization fails, returns the result of <c>ToString()</c> or an empty string.
        /// </returns>
        private string SerializeMessage<T>(T message)
        {
            try
            {
                return JsonSerializer.Serialize(message);
            }
            catch
            {
                return message?.ToString() ?? string.Empty;
            }
        }
    }



}
