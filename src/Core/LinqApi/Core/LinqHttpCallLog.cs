namespace LinqApi.Core
{
    /// <summary>
    /// HTTP call log entity.
    /// </summary>
    public class LinqHttpCallLog : BaseLogEntity
    {
        public string ParentCorrelationId { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public DateTime CreatedAt { get; set; }

        // Additional analytic fields:
        public string Controller { get; set; }
        public string Action { get; set; }
        public string UserAgent { get; set; }
        public string ClientIP { get; set; }

        // Distinguish between different types of logs (e.g. Incoming, Outgoing, Info, etc.)

    }


}

