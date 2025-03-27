using LinqApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace LinqApi.Core
{

    namespace LinqApi.Core
    {
        /// <summary>
        /// Default implementation for logging to MSSQL using EF Core.
        /// </summary>
        /// <summary>
        /// Default implementation for logging to MSSQL using EF Core.
        /// </summary>
        public class LinqDbContextCallLogger : ILinqLogger
        {
            private readonly LinqLoggingDbContext _db;
            private readonly ICorrelationIdGenerator _correlationGenerator;

            public LinqDbContextCallLogger(LinqLoggingDbContext db, ICorrelationIdGenerator correlationGenerator)
            {
                _db = db ?? throw new ArgumentNullException(nameof(db));
                _correlationGenerator = correlationGenerator ?? throw new ArgumentNullException(nameof(correlationGenerator));
            }

            public virtual IDisposable BeginScope<TState>(TState state) where TState : notnull
                => new LoggerScope(() => { });

            public virtual bool IsEnabled(LogLevel logLevel) =>
                logLevel >= LogLevel.Information;

            /// <summary>
            /// Synchronous log metodu. Hata varsa ilgili hata logunu (ör. LinqDatabaseErrorLog, LinqConsumeErrorLog veya LinqPublishErrorLog)
            /// oluşturur, aksi halde standart HTTP call logu (LinqHttpCallLog) üretir.
            /// </summary>
            public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                    return;

                string message = formatter(state, exception);
                string currentCorrelation = CorrelationContext.Current?.ParentId.ToString() ?? Guid.NewGuid().ToString();

                LinqLogEntity logEntry;

                if (exception != null)
                {
                    // Özel durumlara göre hata türünü belirlemek için ilave kontroller ekleyebilirsiniz.
                    // Aşağıdaki örnekte, mesaj içeriğine bağlı olarak consume veya publish error logu üretimi yapılıyor.
                    if (message.Contains("consume", StringComparison.OrdinalIgnoreCase))
                    {
                        logEntry = new LinqConsumeErrorLog
                        {
                            ParentCorrelationId = currentCorrelation,
                            Exception = exception.ToString(),
                            StackTrace = exception.StackTrace,
                            IsException = true,
                            DurationMs = 0,
                            CreatedAt = DateTime.UtcNow,
                            IsInternal = true
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
                            CreatedAt = DateTime.UtcNow,
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
                            CreatedAt = DateTime.UtcNow,
                            IsInternal = true
                        };
                    }
                }
                else
                {
                    // Hata yoksa standart HTTP call logu üretelim.
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
                        CreatedAt = DateTime.UtcNow,
                        IsInternal = true,
                    };
                }

                // Log tipine göre doğru DbSet'e ekleyelim.
                switch (logEntry)
                {
                    case LinqHttpCallLog httpLog:
                        _db.HttpCallLogs.Add(httpLog);
                        break;
                    case LinqSqlErrorLog dbError:
                        _db.LinqDatabaseErrorLogs.Add(dbError);
                        break;
                    case LinqConsumeErrorLog consumeError:
                        _db.LinqConsumeErrorLogs.Add(consumeError);
                        break;
                    case LinqPublishErrorLog publishError:
                        _db.LinqPublishErrorLogs.Add(publishError);
                        break;
                    case LinqEventLog eventLog:
                        _db.EventLogs.Add(eventLog);
                        break;
                    default:
                        _db.Logs.Add(logEntry);
                        break;
                }

                _db.SaveChanges();
            }

            /// <summary>
            /// Asenkron log metodunda, gönderilen LinqLogEntity örneğini uygun DbSet'e ekler.
            /// </summary>
            public virtual async Task LogAsync(LinqLogEntity log, CancellationToken cancellationToken)
            {
                if (log.IsInternal)
                    return;

                switch (log)
                {
                    case LinqHttpCallLog httpLog:
                        _db.HttpCallLogs.Add(httpLog);
                        break;
                    case LinqSqlErrorLog dbError:
                        _db.LinqDatabaseErrorLogs.Add(dbError);
                        break;
                    case LinqConsumeErrorLog consumeError:
                        _db.LinqConsumeErrorLogs.Add(consumeError);
                        break;
                    case LinqPublishErrorLog publishError:
                        _db.LinqPublishErrorLogs.Add(publishError);
                        break;
                    case LinqEventLog eventLog:
                        _db.EventLogs.Add(eventLog);
                        break;
                    case LinqSqlLog linqSqlLog:
                        _db.SqlLogs.Add(linqSqlLog);
                        break;
                    default:
                        _db.Logs.Add(log);
                        break;
                }
                await _db.SaveChangesAsync(cancellationToken);
            }

        }


    }

}