using LinqApi.Correlation;
using LinqApi.Logging;
using MassTransit;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LinqApi.MassTransit
{


    namespace LinqApi.MassTransit
    {
        /// <summary>
        /// A MassTransit consume filter that logs key details from the ConsumeContext.
        /// It uses the CorrelationContext to generate nested correlation IDs with step incrementation
        /// and logs a pre-consume event along with either a success or error event.
        /// </summary>
        /// <typeparam name="T">The message type being consumed.</typeparam>
        public class LinqLoggingConsumeFilter<T> : IFilter<ConsumeContext<T>> where T : class
        {
            private readonly ILinqLogger _logger;
            private readonly ICorrelationIdGenerator _correlationGenerator;
            private readonly IOptions<LinqLoggingOptions> _options;

            public LinqLoggingConsumeFilter(
                ILinqLogger logger,
                ICorrelationIdGenerator correlationGenerator,
                IOptions<LinqLoggingOptions> options)
            {
                _logger = logger;
                _correlationGenerator = correlationGenerator;
                _options = options;
            }

            public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
            {
                // Use the header name from options.
                string correlationHeader = _options.Value.CorrelationHeaderName;
                string headerCorrelation = null;
                if (context.Headers.TryGetHeader(correlationHeader, out var existing))
                {
                    headerCorrelation = existing?.ToString();
                }
                else
                {
                    CorrelationContext.EnsureCorrelation(_correlationGenerator);
                    headerCorrelation = CorrelationContext.Current?.ParentId.ToString()
                                        ?? _correlationGenerator.Generate(1, 1).ToString();
                    // Use SetHeader if Set is not available:

                }

                // Generate a new child correlation ID.

                // Log a pre-consume event.
                var preLog = new LinqEventLog
                {
                    OperationName = $"Consume-{typeof(T).Name}",
                    RequestPayload = SerializeMessage(context.Message),
                    ResponsePayload = string.Empty,
                    QueueName = context.ReceiveContext.InputAddress?.ToString() ?? "Unknown",
                    Success = false,
                    CreatedAt = DateTime.UtcNow,
                };

                await _logger.LogAsync(preLog, context.CancellationToken);

                try
                {
                    await next.Send(context);

                    // Log a success event.
                    var successLog = new LinqEventLog
                    {
                        OperationName = $"Consume-{typeof(T).Name}",
                        RequestPayload = SerializeMessage(context.Message),
                        ResponsePayload = "Message consumed successfully",
                        QueueName = context.ReceiveContext.InputAddress?.ToString() ?? "Unknown",
                        Success = true,
                        CreatedAt = DateTime.UtcNow,
                    };

                    await _logger.LogAsync(ConvertToHttpCallLog(successLog), context.CancellationToken);
                }
                catch (Exception ex)
                {
                    // Log an error event.
                    var errorLog = new LinqEventLog
                    {
                        OperationName = $"Consume-{typeof(T).Name}",
                        RequestPayload = SerializeMessage(context.Message),
                        ResponsePayload = ex.ToString(),
                        QueueName = context.ReceiveContext.InputAddress?.ToString() ?? "Unknown",
                        Success = false,
                        CreatedAt = DateTime.UtcNow,
                    };

                    await _logger.LogAsync(errorLog, context.CancellationToken);
                    throw;
                }
            }

            public void Probe(ProbeContext context) => context.CreateScope("LinqLoggingConsumeFilter");

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

            // Helper to convert a LinqEventLog to a LinqHttpCallLog.
            private LinqHttpCallLog ConvertToHttpCallLog(LinqEventLog ev) => new LinqHttpCallLog
            {
                ParentCorrelationId = "", // Optionally map if you have one.
                Url = ev.QueueName,         // Using the queue as an identifier.
                Method = ev.OperationName,
                RequestBody = ev.RequestPayload,
                ResponseBody = ev.ResponsePayload,
                DurationMs = 0,
                Exception = ev.Exception,
                CreatedAt = ev.CreatedAt,
                IsInternal = true,
                IsException = string.IsNullOrEmpty(ev.Exception),
            };
        }
    }

}
