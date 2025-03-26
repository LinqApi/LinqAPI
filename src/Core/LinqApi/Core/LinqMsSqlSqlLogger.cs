namespace LinqApi.Core
{
    /// <summary>
    /// Default implementation that logs SQL entries to MSSQL using EF Core.
    /// </summary>
    public class LinqMsSqlSqlLogger : ILinqSqlLogger
    {
        private readonly LinqLoggingDbContext _db;
        public LinqMsSqlSqlLogger(LinqLoggingDbContext db)
        {
            _db = db;
        }
        public async Task LogSqlAsync(LinqSqlLog sqlLog, CancellationToken cancellationToken)
        {
            _db.Logs.Add(sqlLog);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }


}

