using LinqApi.Logging.Log;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinqApi.Logging
{
    // HTTP log
    /// <summary>
    /// Represents a log entry for an HTTP request/response cycle, capturing both input and output payloads,
    /// metadata such as controller/action names, and status codes.
    /// </summary>
    /// <remarks>
    /// This log is useful for tracing API activity, auditing, debugging issues,
    /// or feeding monitoring/observability systems.
    /// </remarks>

    [Table("HttpCallLogs", Schema = "log")]
    public class LinqHttpCallLog : LinqLogEntity
    {
        /// <summary>
        /// Gets or sets the full URL of the HTTP request.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the HTTP method used (e.g., GET, POST, PUT, DELETE).
        /// </summary>
        public string? Method { get; set; }

        /// <summary>
        /// Gets or sets the body content of the request.
        /// </summary>
        public string? RequestBody { get; set; }

        /// <summary>
        /// Gets or sets the response body returned by the server.
        /// </summary>
        public string? ResponseBody { get; set; }

        /// <summary>
        /// Gets or sets the name of the controller handling the request, if applicable.
        /// </summary>
        public string? Controller { get; set; }

        /// <summary>
        /// Gets or sets the name of the action method within the controller, if applicable.
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code returned in the response.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the value of the User-Agent header from the request.
        /// </summary>
        public string? UserAgent { get; set; }

      
    }
}

