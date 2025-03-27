using LinqApi.Logging;
using MassTransit;

namespace LinqApi.MassTransit
{
    public class LinqLogMessageConsumer : IConsumer<ILogMessage>
    {
        private readonly ILinqLogger _logger;

        public LinqLogMessageConsumer(ILinqLogger logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ILogMessage> context)
        {
            var message = context.Message;

            // Map the incoming message to your log entity.
            // Here we assume that LogType in the message distinguishes between API logs and HTTP call logs.
            var logEntry = new LinqEventLog
            {
                ParentCorrelationId = message.ParentCorrelationId,
                DurationMs = 0, // you might want to calculate if available
                Exception = null,
                CreatedAt = message.CreatedAt,
                IsInternal = false,
            };

            // Log the message using your logger (which in turn writes to the logging DbContext or outbox)
            await _logger.LogAsync(logEntry, context.CancellationToken);
        }
    }
}
