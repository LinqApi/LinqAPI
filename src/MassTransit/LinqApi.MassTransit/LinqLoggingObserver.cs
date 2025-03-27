using LinqApi.Core;
using LinqApi.Model;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace LinqApi.MassTransit
{
    /// <summary>
    /// Global publish ve consume işlemleri için observer. 
    /// Hem IPublishObserver hem de IConsumeObserver implement eder.
    /// Publish veya consume sırasında pre, post ve fault olaylarını yakalayıp log kaydı oluşturur.
    /// </summary>
    //public class LinqLoggingObserver : IPublishObserver, IConsumeObserver
    //{
    //    private readonly ILinqLogger _logger;
    //    private readonly ICorrelationIdGenerator _correlationGenerator;
    //    private readonly IOptions<LinqLoggingOptions> _options;

    //    public LinqLoggingObserver(
    //        ILinqLogger logger,
    //        ICorrelationIdGenerator correlationGenerator,
    //        IOptions<LinqLoggingOptions> options)
    //    {
    //        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    //        _correlationGenerator = correlationGenerator ?? throw new ArgumentNullException(nameof(correlationGenerator));
    //        _options = options ?? throw new ArgumentNullException(nameof(options));
    //    }

    //    #region IPublishObserver Members

    //    public async Task PrePublish<T>(PublishContext<T> context) where T : class
    //    {
    //        // Ensure correlation header exists
    //        string correlationHeader = _options.Value.CorrelationHeaderName;
    //        if (!context.Headers.TryGetHeader(correlationHeader, out var existing))
    //        {
    //            CorrelationContext.EnsureCorrelation(_correlationGenerator);
    //            var headerCorrelation = CorrelationContext.Current?.ParentId.ToString()
    //                                    ?? _correlationGenerator.Generate(1, 1).ToString();
    //            context.Headers.Set(correlationHeader, headerCorrelation);
    //        }

    //        // Generate a new child correlation id
    //        string currentStepCorrelation = CorrelationContext.GetNextCorrelationId(_correlationGenerator);

    //        var preLog = new LinqEventLog
    //        {
    //            CorrelationId = currentStepCorrelation,
    //            OperationName = $"Publish-{typeof(T).Name}",
    //            RequestPayload = SerializeMessage(context.Message),
    //            ResponsePayload = string.Empty,
    //            QueueName = context.DestinationAddress?.ToString() ?? "Unknown",
    //            Success = false,
    //            CreatedAt = DateTime.UtcNow
    //        };

    //        await _logger.LogAsync(preLog, context.CancellationToken);
    //    }

    //    public async Task PostPublish<T>(PublishContext<T> context) where T : class
    //    {
    //        string currentStepCorrelation = CorrelationContext.GetNextCorrelationId(_correlationGenerator);
    //        var successLog = new LinqEventLog
    //        {
    //            CorrelationId = currentStepCorrelation,
    //            OperationName = $"Publish-{typeof(T).Name}",
    //            RequestPayload = SerializeMessage(context.Message),
    //            ResponsePayload = "Message published successfully",
    //            QueueName = context.DestinationAddress?.ToString() ?? "Unknown",
    //            Success = true,
    //            CreatedAt = DateTime.UtcNow
    //        };

    //        await _logger.LogAsync(successLog, context.CancellationToken);
    //    }

    //    public async Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
    //    {
    //        string currentStepCorrelation = CorrelationContext.GetNextCorrelationId(_correlationGenerator);
    //        var faultLog = new LinqEventLog
    //        {
    //            CorrelationId = currentStepCorrelation,
    //            OperationName = $"Publish-{typeof(T).Name}",
    //            RequestPayload = SerializeMessage(context.Message),
    //            ResponsePayload = exception.ToString(),
    //            QueueName = context.DestinationAddress?.ToString() ?? "Unknown",
    //            Success = false,
    //            CreatedAt = DateTime.UtcNow
    //        };

    //        await _logger.LogAsync(faultLog, context.CancellationToken);
    //    }

    //    #endregion

    //    #region IConsumeObserver Members

    //    public async Task PreConsume<T>(ConsumeContext<T> context) where T : class
    //    {
    //        string correlationHeader = _options.Value.CorrelationHeaderName;
    //        if (!context.Headers.TryGetHeader(correlationHeader, out var existing))
    //        {
    //            CorrelationContext.EnsureCorrelation(_correlationGenerator);
    //            var headerCorrelation = CorrelationContext.Current?.ParentId.ToString()
    //                                    ?? _correlationGenerator.Generate(1, 1).ToString();
    //            // ConsumeContext genelde header set etmeye izin vermez; burada sadece kontrol amaçlı.
    //        }

    //        string currentStepCorrelation = CorrelationContext.GetNextCorrelationId(_correlationGenerator);
    //        var preLog = new LinqEventLog
    //        {
    //            CorrelationId = currentStepCorrelation,
    //            OperationName = $"Consume-{typeof(T).Name}",
    //            RequestPayload = SerializeMessage(context.Message),
    //            ResponsePayload = string.Empty,
    //            QueueName = context.ReceiveContext.InputAddress?.ToString() ?? "Unknown",
    //            Success = false,
    //            CreatedAt = DateTime.UtcNow
    //        };

    //        await _logger.LogAsync(preLog, context.CancellationToken);
    //    }

    //    public async Task PostConsume<T>(ConsumeContext<T> context) where T : class
    //    {
    //        string currentStepCorrelation = CorrelationContext.GetNextCorrelationId(_correlationGenerator);
    //        var successLog = new LinqEventLog
    //        {
    //            CorrelationId = currentStepCorrelation,
    //            OperationName = $"Consume-{typeof(T).Name}",
    //            RequestPayload = SerializeMessage(context.Message),
    //            ResponsePayload = "Message consumed successfully",
    //            QueueName = context.ReceiveContext.InputAddress?.ToString() ?? "Unknown",
    //            Success = true,
    //            CreatedAt = DateTime.UtcNow
    //        };

    //        await _logger.LogAsync(successLog, context.CancellationToken);
    //    }

    //    public async Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
    //    {
    //        string currentStepCorrelation = CorrelationContext.GetNextCorrelationId(_correlationGenerator);
    //        var faultLog = new LinqEventLog
    //        {
    //            CorrelationId = currentStepCorrelation,
    //            OperationName = $"Consume-{typeof(T).Name}",
    //            RequestPayload = SerializeMessage(context.Message),
    //            ResponsePayload = exception.ToString(),
    //            QueueName = context.ReceiveContext.InputAddress?.ToString() ?? "Unknown",
    //            Success = false,
    //            CreatedAt = DateTime.UtcNow
    //        };

    //        await _logger.LogAsync(faultLog, context.CancellationToken);
    //    }

    //    #endregion

    //    public void Probe(ProbeContext context) => context.CreateScope("LinqLoggingObserver");

    //    private string SerializeMessage<T>(T message)
    //    {
    //        try
    //        {
    //            return JsonSerializer.Serialize(message);
    //        }
    //        catch
    //        {
    //            return message?.ToString() ?? string.Empty;
    //        }
    //    }
    //}


    public class LinqLoggingObserver : IPublishObserver, IConsumeObserver
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICorrelationIdGenerator _correlationGenerator;
        private readonly IOptions<LinqLoggingOptions> _options;

        public LinqLoggingObserver(
            IServiceScopeFactory scopeFactory,
            ICorrelationIdGenerator correlationGenerator,
            IOptions<LinqLoggingOptions> options)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _correlationGenerator = correlationGenerator ?? throw new ArgumentNullException(nameof(correlationGenerator));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        private async Task LogAsync(LinqLogEntity logEntry, CancellationToken cancellationToken = default)
        {
            // Yeni bir scope oluşturarak ILinqLogger'ı scoped olarak çöz
            using var scope = _scopeFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILinqLogger>();
            await logger.LogAsync(logEntry, cancellationToken);
        }

        #region IPublishObserver Members

        public async Task PrePublish<T>(PublishContext<T> context) where T : class
        {
            string correlationHeader = _options.Value.CorrelationHeaderName;
            if (!context.Headers.TryGetHeader(correlationHeader, out var existing))
            {
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

            await LogAsync(preLog);
        }

        public async Task PostPublish<T>(PublishContext<T> context) where T : class
        {
            var successLog = new LinqEventLog
            {
                RequestPayload = SerializeMessage(context.Message),
                ResponsePayload = "Message published successfully",
                QueueName = context.DestinationAddress?.ToString() ?? "Unknown",
                Success = true,
                CreatedAt = DateTime.UtcNow
            };

            await LogAsync(successLog);
        }

        public async Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
            var faultLog = new LinqPublishErrorLog
            {

                IsException = true,
                CreatedAt = DateTime.UtcNow,
                StackTrace = exception.StackTrace,
            };

            await LogAsync(faultLog);
        }

        #endregion

        #region IConsumeObserver Members

        public async Task PreConsume<T>(ConsumeContext<T> context) where T : class
        {
            string correlationHeader = _options.Value.CorrelationHeaderName;
            if (!context.Headers.TryGetHeader(correlationHeader, out var existing))
            {
                CorrelationContext.EnsureCorrelation(_correlationGenerator);
                var headerCorrelation = CorrelationContext.Current?.ParentId.ToString()
                                        ?? _correlationGenerator.Generate(1, 1).ToString();
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

            await LogAsync(preLog);
        }

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

            await LogAsync(successLog);
        }

        public async Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
        {
            var faultLog = new LinqConsumeErrorLog
            {
                IsException = true,
                CreatedAt = DateTime.UtcNow,
                StackTrace = exception.StackTrace,
            };

            await LogAsync(faultLog);
        }

        #endregion

        public void Probe(ProbeContext context) => context.CreateScope("LinqLoggingObserver");

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
