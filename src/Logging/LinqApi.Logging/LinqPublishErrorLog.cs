namespace LinqApi.Logging
{
    // Örneğin, Publish error log
    public class LinqPublishErrorLog : LinqErrorLog
    {
        public override string LogType { get; set; } = "PublishError";
    }
}

