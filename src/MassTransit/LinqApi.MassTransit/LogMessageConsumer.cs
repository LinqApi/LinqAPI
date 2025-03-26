using LinqApi.Core;
using MassTransit;

namespace LinqApi.MassTransit
{
    public class LinqLogMessageConsumer : IConsumer<ILogMessage>
    {
        private readonly ILinqHttpCallLogger _logger;

        public LinqLogMessageConsumer(ILinqHttpCallLogger logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ILogMessage> context)
        {
            var message = context.Message;

            // Map the incoming message to your log entity.
            // Here we assume that LogType in the message distinguishes between API logs and HTTP call logs.
            var logEntry = new LinqHttpCallLog
            {
                CorrelationId = message.CorrelationId,
                ParentCorrelationId = message.ParentCorrelationId,
                Url = message.MessageType == "ApiLog" ? "API Request" : "HTTP Call",
                Method = message.MessageType, // can be further customized
                RequestBody = message.Payload,
                ResponseBody = string.Empty,
                DurationMs = 0, // you might want to calculate if available
                Exception = null,
                CreatedAt = message.CreatedAt,
                IsInternal = false,
                LogType = message.MessageType == "ApiLog" ? LogType.Incoming : LogType.Outgoing
            };

            // Log the message using your logger (which in turn writes to the logging DbContext or outbox)
            await _logger.LogAsync(logEntry);
        }
    }
}
