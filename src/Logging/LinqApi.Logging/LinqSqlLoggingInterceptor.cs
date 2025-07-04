namespace LinqApi.Logging
{
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using System.Collections.Concurrent;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System;

    namespace LinqApi.Core
    {
        /// <summary>
        /// An EF Core interceptor that logs SQL command execution details.
        /// It captures the start time in the executing events and then computes the duration when the command finishes.
        /// </summary>
        public class LinqSqlLoggingInterceptor : DbCommandInterceptor
        {
            // A thread-safe dictionary to store Stopwatch for each command.
            private readonly ConcurrentDictionary<DbCommand, Stopwatch> _stopwatches = new();

            private readonly ILinqLogger _linqLogger;
            private readonly IUserContext<string> userContext;

            public LinqSqlLoggingInterceptor(ILinqLogger linqLogger, IUserContext<string> userContext)
            {
                _linqLogger = linqLogger;
                this.userContext = userContext;
            }

            // Synchronous executing methods:
            public override InterceptionResult<DbDataReader> ReaderExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result)
            {
                StartStopwatch(command);
                return base.ReaderExecuting(command, eventData, result);
            }

            public override InterceptionResult<int> NonQueryExecuting(
       DbCommand command,
       CommandEventData eventData,
       InterceptionResult<int> result)
            {
                return base.NonQueryExecuting(command, eventData, result);
            }

            public override InterceptionResult<object> ScalarExecuting(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object> result)
            {
                StartStopwatch(command);
                return base.ScalarExecuting(command, eventData, result);
            }

            // Synchronous executed methods:
            public override async ValueTask<DbDataReader> ReaderExecutedAsync(
         DbCommand command,
         CommandExecutedEventData eventData,
         DbDataReader result, CancellationToken cancellationToken = default)
            {
                var duration = StopAndRemoveStopwatch(command);
                var cmdType = "Reader";
                if (command.Transaction == null)
                {
                    // Bu kısım, EFCoreSecondLevelCacheInterceptor devredeyken ve önbellek isabet ettiğinde ÇAĞRILMAYACAĞINI UNUTMAYIN.
                    // Sadece gerçek veritabanı sorgularının Transaction'ı null olduğunda (yani implicit transaction veya hiç transaction yoksa) devreye girer.
                    cmdType = "Cache";
                }
                await LogSql(command, duration, cmdType, eventData, cancellationToken);
                return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
            }

            public override async ValueTask<int> NonQueryExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                int result, CancellationToken cancellationToken = default)
            {
                var duration = StopAndRemoveStopwatch(command);
                var cmdType = "NonQuery";
                if (command.Transaction == null)
                {
                    cmdType = "Cache";
                }
                await LogSql(command, duration, cmdType, eventData, cancellationToken);
                return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
            }

            public override async ValueTask<object> ScalarExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                object result, CancellationToken cancellationToken = default)
            {
                var duration = StopAndRemoveStopwatch(command);
                var cmdType = "Scalar";
                if (command.Transaction == null)
                {
                    cmdType = "Cache";
                }
                await LogSql(command, duration, cmdType, eventData, cancellationToken);
                return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
            }


            // Asynchronous versions:
            public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<DbDataReader> result,
                CancellationToken cancellationToken = default)
            {
                StartStopwatch(command);
                return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
            }

            public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<int> result,
                CancellationToken cancellationToken = default)
            {
                StartStopwatch(command);
               
                return await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
            }

            public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
                DbCommand command,
                CommandEventData eventData,
                InterceptionResult<object> result,
                CancellationToken cancellationToken = default)
            {
                StartStopwatch(command);
                return await base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
            }



            // Helper to start a stopwatch for a given command.
            private void StartStopwatch(DbCommand command)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                _stopwatches[command] = stopwatch;
            }

            // Helper to stop the stopwatch and remove it from the dictionary.
            private long StopAndRemoveStopwatch(DbCommand command)
            {
                if (_stopwatches.TryRemove(command, out Stopwatch stopwatch))
                {
                    stopwatch.Stop();
                    return stopwatch.ElapsedMilliseconds;
                }
                return 0;
            }

            private async Task LogSql(DbCommand command, long durationMs, string commandType, CommandExecutedEventData eventData, CancellationToken cancellationToken)
            {
                // RECURSIVE LOGGING GUARD:
                if (command.CommandText.Contains("log.", StringComparison.OrdinalIgnoreCase))
                    return;

                var sqlLog = new LinqSqlLog
                {
                    QueryText = command.CommandText,
                    DurationMs = durationMs,
                    UserId = userContext.Id,
                    ExecutedAt = DateTime.UtcNow,
                    CommandType = commandType,
                    
                };

                await _linqLogger.LogAsync(sqlLog, cancellationToken).ConfigureAwait(false);
            }
        }
    }

}
