using LinqApi.Core.Log;

namespace LinqApi.Logging
{
    // HTTP log
    public class LinqHttpCallLog : LinqLogEntity
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public int StatusCode { get; set; }

        public override string LogType { get; set; } = "HttpCall";
        public string UserAgent { get; set; }
    }
}

