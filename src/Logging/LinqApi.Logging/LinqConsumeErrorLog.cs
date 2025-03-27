namespace LinqApi.Logging
{
    // Örneğin, Consume error log
    public class LinqConsumeErrorLog : LinqErrorLog
    {
        public override string LogType { get; set; } = "ConsumeError";
    }
}

