namespace LinqApi.Core
{
    // Örneğin, Database error log
    public class LinqSqlErrorLog : LinqErrorLog
    {
    }

    public class OutboundHttpCallError : LinqErrorLog
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string UserAgent { get; set; }
    }

    public class HttpCallInboundError : LinqErrorLog
    {
    }
}

