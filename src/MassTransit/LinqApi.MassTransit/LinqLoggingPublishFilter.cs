using LinqApi.Core;
using MassTransit;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LinqApi.MassTransit
{


    namespace LinqApi.MassTransit
    {
        /// <summary>
        /// A MassTransit publish filter that logs key SendContext details.
        /// It logs a pre-send event and then logs either a success or error event.
        /// The filter uses the CorrelationContext to generate a nested correlation ID (with step incrementation).
        /// </summary>
        /// <typeparam name="T">The message type being published.</typeparam>
        public class LinqLoggingPublishFilter<T> : IFilter<SendContext<T>> where T : class
        {
            private readonly ILinqHttpCallLogger _logger;
            private readonly ICorrelationIdGenerator _correlationGenerator;
            private readonly IOptions<LinqLoggingOptions> _options;

            public LinqLoggingPublishFilter(
                ILinqHttpCallLogger logger,
                ICorrelationIdGenerator correlationGenerator,
                IOptions<LinqLoggingOptions> options)
            {
                _logger = logger;
                _correlationGenerator = correlationGenerator;
                _options = options;
            }

            public async Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
            {
                // Use the configured header name for correlation IDs.
                string correlationHeader = _options.Value.CorrelationHeaderName;
                string headerCorrelation = null;
                if (context.Headers.TryGetHeader(correlationHeader, out var existing))
                {
                    headerCorrelation = existing?.ToString();
                }
                else
                {
                    // Ensure a correlation context exists.
                    CorrelationContext.EnsureCorrelation(_correlationGenerator);
                    headerCorrelation = CorrelationContext.Current?.ParentId.ToString()
                                        ?? _correlationGenerator.Generate(1, 1).ToString();
                    context.Headers.Set(correlationHeader, headerCorrelation);
                }

                // Generate a new child correlation id (increments the step).
                string currentStepCorrelation = CorrelationContext.GetNextCorrelationId(_correlationGenerator);

                // Create a pre-send log event using selected SendContext details.
                var preLog = new LinqEventLog
                {
                    CorrelationId = currentStepCorrelation,
                    // Here, OperationName serves as a discriminator; you can include the message type.
                    OperationName = $"Publish-{typeof(T).Name}",
                    RequestPayload = SerializeMessage(context.Message),
                    ResponsePayload = string.Empty,
                    // Use the destination address as the QueueName for identification.
                    QueueName = context.DestinationAddress?.ToString() ?? "Unknown",
                    Success = false, // Pre-send event: not complete yet.
                    CreatedAt = DateTime.UtcNow,
                    LogType = LogType.Info
                };

                await _logger.LogAsync(ConvertToHttpCallLog(preLog));

                try
                {
                    await next.Send(context);

                    // Log success event.
                    var successLog = new LinqEventLog
                    {
                        CorrelationId = currentStepCorrelation,
                        OperationName = $"Publish-{typeof(T).Name}",
                        RequestPayload = SerializeMessage(context.Message),
                        ResponsePayload = "Message published successfully",
                        QueueName = context.DestinationAddress?.ToString() ?? "Unknown",
                        Success = true,
                        CreatedAt = DateTime.UtcNow,
                        LogType = LogType.Info
                    };

                    await _logger.LogAsync(ConvertToHttpCallLog(successLog));
                }
                catch (Exception ex)
                {
                    // Log error event.
                    var errorLog = new LinqEventLog
                    {
                        CorrelationId = currentStepCorrelation,
                        OperationName = $"Publish-{typeof(T).Name}",
                        RequestPayload = SerializeMessage(context.Message),
                        ResponsePayload = ex.ToString(),
                        QueueName = context.DestinationAddress?.ToString() ?? "Unknown",
                        Success = false,
                        CreatedAt = DateTime.UtcNow,
                        LogType = LogType.Error
                    };

                    await _logger.LogAsync(ConvertToHttpCallLog(errorLog));
                    throw;
                }
            }

            public void Probe(ProbeContext context) => context.CreateScope("LinqLoggingPublishFilter");

            private string SerializeMessage(T message)
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

            // This helper converts a LinqEventLog to a LinqHttpCallLog,
            // assuming your ILinqHttpCallLogger logs a BaseLogEntity.
            private LinqHttpCallLog ConvertToHttpCallLog(LinqEventLog ev) => new LinqHttpCallLog
            {
                CorrelationId = ev.CorrelationId,
                ParentCorrelationId = "", // You could map additional fields if needed.
                Url = ev.QueueName,         // In this context, the "QueueName" holds the destination.
                Method = ev.OperationName,
                RequestBody = ev.RequestPayload,
                ResponseBody = ev.ResponsePayload,
                DurationMs = 0,
                Exception = ev.Exception,
                CreatedAt = ev.CreatedAt,
                IsInternal = true,
                LogType = ev.LogType
            };
        }
    }

}
