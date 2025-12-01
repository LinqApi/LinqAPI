using LinqApi.Core.Log;
using LinqApi.Correlation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinqApi.Core
{
    public class LinqDbContextCallLogger : ILinqLogger
    {
        private readonly LinqLoggingDbContext _db;
        private readonly IOptions<LinqLoggingOptions> _options;

        public LinqDbContextCallLogger(
            LinqLoggingDbContext db,
            ICorrelationIdGenerator correlationGenerator,
            IOptions<LinqLoggingOptions> options)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _options = options;
        }

        public virtual IDisposable BeginScope<TState>(TState state) where TState : notnull
            => new LoggerScope(() => { });

        public virtual bool IsEnabled(LogLevel logLevel)
        {
            // Eğer sadece Error loglamak isteniyorsa
            if (_options.Value.LogLevel.Equals("Error", StringComparison.OrdinalIgnoreCase))
                return logLevel >= LogLevel.Error;

            // Aksi halde Information ve üzerini logla
            return logLevel >= LogLevel.Information;
        }

        public virtual void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            // Eğer bu seviyede loglamaya izin yoksa
            if (!IsEnabled(logLevel))
                return;

            // Eğer "Error" modundaysa ve exception yoksa çık
            if (_options.Value.LogLevel.Equals("Error", StringComparison.OrdinalIgnoreCase)
                && exception == null)
                return;

            string message = formatter(state, exception);
            string currentCorrelation = CorrelationContext.Current?.ParentId.ToString()
                                         ?? Guid.NewGuid().ToString();

            LinqLogEntity logEntry;

            if (exception != null)
            {
                // Hata logları
                if (message.Contains("consume", StringComparison.OrdinalIgnoreCase))
                {
                    logEntry = new LinqConsumeErrorLog
                    {
                        ParentCorrelationId = currentCorrelation,
                        Exception = exception.ToString(),
                        StackTrace = exception.StackTrace,
                        IsException = true,
                        DurationMs = 0,
                        CreatedAt = DateTime.Now,
                        IsInternal = true,
                    };
                }
                else if (message.Contains("publish", StringComparison.OrdinalIgnoreCase))
                {
                    logEntry = new LinqPublishErrorLog
                    {
                        ParentCorrelationId = currentCorrelation,
                        Exception = exception.ToString(),
                        StackTrace = exception.StackTrace,
                        IsException = true,
                        DurationMs = 0,
                        CreatedAt = DateTime.Now,
                        IsInternal = true
                    };
                }
                else
                {
                    logEntry = new LinqSqlErrorLog
                    {
                        ParentCorrelationId = currentCorrelation,
                        Exception = exception.ToString(),
                        StackTrace = exception.StackTrace,
                        IsException = true,
                        DurationMs = 0,
                        CreatedAt = DateTime.Now,
                        IsInternal = true
                    };
                }
            }
            else
            {
                // Normal HTTP-LOG (Information) logu
                logEntry = new LinqHttpCallLog
                {
                    ParentCorrelationId = currentCorrelation,
                    Url = "N/A",
                    Method = "LOG",
                    RequestBody = message,
                    ResponseBody = string.Empty,
                    DurationMs = 0,
                    Exception = null,
                    IsException = false,
                    CreatedAt = DateTime.Now,
                    IsInternal = true,
                };
            }

            // Doğru DbSet'e ekle
            switch (logEntry)
            {
                case LinqHttpCallLog httpLog: _db.HttpCallLogs.Add(httpLog); break;
                case LinqSqlErrorLog dbError: _db.LinqDatabaseErrorLogs.Add(dbError); break;
                case LinqConsumeErrorLog consumeErr: _db.LinqConsumeErrorLogs.Add(consumeErr); break;
                case LinqPublishErrorLog publishErr: _db.LinqPublishErrorLogs.Add(publishErr); break;
                case LinqEventLog eventLog: _db.EventLogs.Add(eventLog); break;
                case LinqSqlLog sqlLog: _db.SqlLogs.Add(sqlLog); break;
                default: break;
            }

            _db.SaveChanges();
        }

        public virtual async Task LogAsync(LinqLogEntity log, CancellationToken cancellationToken)
        {
            if (log.IsInternal)
                return;

            switch (log)
            {
                case LinqHttpCallLog httpLog: await _db.HttpCallLogs.AddAsync(httpLog, cancellationToken); break;
                case LinqSqlErrorLog dbError: await _db.LinqDatabaseErrorLogs.AddAsync(dbError, cancellationToken); break;
                case LinqConsumeErrorLog consumeErr: await _db.LinqConsumeErrorLogs.AddAsync(consumeErr, cancellationToken); break;
                case LinqPublishErrorLog publishErr: await _db.LinqPublishErrorLogs.AddAsync(publishErr, cancellationToken); break;
                case LinqEventLog eventLog: await _db.EventLogs.AddAsync(eventLog, cancellationToken); break;
                case LinqSqlLog sqlLog: await _db.SqlLogs.AddAsync(sqlLog, cancellationToken); break;
                default: break;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}