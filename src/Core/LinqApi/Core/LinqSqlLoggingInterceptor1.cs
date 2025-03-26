//using System.Diagnostics;
//using Microsoft.EntityFrameworkCore.Diagnostics;
//using System.Data.Common;

//namespace LinqApi.Core
//{
//    /// <summary>
//    /// An EF Core interceptor that logs SQL command execution details.
//    /// </summary>
//    public class LinqSqlLoggingInterceptor : DbCommandInterceptor
//    {
//        private readonly ILinqSqlLogger _sqlLogger;

//        public LinqSqlLoggingInterceptor(ILinqSqlLogger sqlLogger)
//        {
//            _sqlLogger = sqlLogger;
//        }


//        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
//            DbCommand command,
//            CommandEventData eventData,
//            InterceptionResult<DbDataReader> result,
//            CancellationToken cancellationToken = default)
//        {
//            var stopwatch = Stopwatch.StartNew();
//            var execResult = await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
//            stopwatch.Stop();
//            await LogSqlAsync(command, stopwatch.ElapsedMilliseconds, "Reader", cancellationToken);
//            return execResult;
//        }


//        public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
//        {
//            //return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);

//            var stopwatch = Stopwatch.StartNew();
//            var execResult = await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
//            stopwatch.Stop();
//            await LogSqlAsync(command, stopwatch.ElapsedMilliseconds, "NonQuery", cancellationToken);
//            return result;
//        }


//        public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
//        {
//            var stopwatch = Stopwatch.StartNew();
//            var execResult = await base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
//            stopwatch.Stop();
//            await LogSqlAsync(command, stopwatch.ElapsedMilliseconds, "Scalar", cancellationToken);
//            return result;
//        }


//        private async Task LogSqlAsync(DbCommand command, long durationMs, string commandType, CancellationToken cancellationToken)
//        {
//            var sqlLog = new LinqSqlLog
//            {
//                QueryText = command.CommandText,
//                DurationMs = durationMs,
//                UserId = UserContext.CurrentUserId, // Retrieve current user id (see UserContext below)
//                ExecutedAt = DateTime.UtcNow,
//                CommandType = commandType,
//                LogType = LogType.Database
//            };

//            await _sqlLogger.LogSqlAsync(sqlLog, cancellationToken);
//        }
//    }


//}

