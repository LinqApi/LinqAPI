namespace LinqApi.Core
{
    /// <summary>
    /// Interface for logging SQL commands.
    /// </summary>
    public interface ILinqSqlLogger
    {
        Task LogSqlAsync(LinqSqlLog sqlLog, CancellationToken cancellationToken);
    }


}

