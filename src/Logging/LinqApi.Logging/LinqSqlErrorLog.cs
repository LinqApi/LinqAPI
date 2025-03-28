using System;

namespace LinqApi.Logging
{
    // Örneğin, Database error log
    public class LinqSqlErrorLog : LinqErrorLog
    {
        public override string LogType { get; set; } = "DatabaseError";
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
        public override string LogType { get; set; } = "OutboundHttpCallError";
    }

    public class HttpCallInboundError : LinqErrorLog
    {
        public override string LogType { get; set; } = "InboudHttpCallError";
    }
}

