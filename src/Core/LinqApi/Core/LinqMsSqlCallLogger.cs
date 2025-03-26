using Microsoft.Extensions.Logging;

namespace LinqApi.Core
{
    /// <summary>
    /// Default implementation for logging to MSSQL using EF Core.
    /// </summary>
    public class LinqMsSqlCallLogger : ILinqHttpCallLogger
    {
        private readonly LinqLoggingDbContext _db;
        public LinqMsSqlCallLogger(LinqLoggingDbContext db)
        {
            _db = db;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new LoggerScope(() => { });

        public bool IsEnabled(LogLevel logLevel) =>
            // For example, enable logging for Information level and above.
            logLevel >= LogLevel.Information;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            // Create a log message using the formatter.
            string message = formatter(state, exception);
            var currentCorrelation = CorrelationContext.Current?.ParentId.ToString() ?? Guid.NewGuid().ToString();
            var logEntry = new LinqHttpCallLog
            {
                // Use the current correlation context to get the next step.
                CorrelationId = CorrelationContext.GetNextCorrelationId(new DefaultCorrelationIdGenerator()),
                ParentCorrelationId = currentCorrelation,
                Url = "N/A",
                Method = "LOG",
                RequestBody = message,
                ResponseBody = string.Empty,
                DurationMs = 0,
                Exception = exception?.ToString(),
                IsException = exception !=null,
                CreatedAt = DateTime.UtcNow,
                IsInternal = true,
                // Set LogType to Info (or an appropriate enum value) if you wish
                LogType = LogType.Info
            };

            _db.Logs.Add(logEntry);
            _db.SaveChanges();
        }

        public async Task LogAsync(LinqHttpCallLog log)
        {
            // Prevent circular logging if this is an internal log.
            if (log.IsInternal)
                return;

            _db.Logs.Add(log);
            await _db.SaveChangesAsync();
        }
    }


}

