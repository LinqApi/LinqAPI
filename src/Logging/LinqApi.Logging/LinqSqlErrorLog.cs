namespace LinqApi.Logging
{
    // Örneğin, Database error log
    public class LinqSqlErrorLog : LinqErrorLog
    {
        public override string LogType { get; set; } = "DatabaseError";
    }
}

