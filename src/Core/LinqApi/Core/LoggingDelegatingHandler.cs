using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace LinqApi.Core
{
    /// <summary>
    /// Delegating handler that logs HTTP requests/responses.
    /// </summary>
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILinqHttpCallLogger _logger;
        private readonly ILinqPayloadMasker _masker;
        private readonly ICorrelationIdGenerator _correlationGenerator;
        private readonly IOptions<LinqLoggingOptions> _options;

        public LoggingDelegatingHandler(
            ILinqHttpCallLogger logger,
            ILinqPayloadMasker masker,
            ICorrelationIdGenerator correlationGenerator,
            IOptions<LinqLoggingOptions> options)
        {
            _logger = logger;
            _masker = masker;
            _correlationGenerator = correlationGenerator;
            _options = options;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            string reqBody = request.Content != null ? await request.Content.ReadAsStringAsync() : null;
            string resBody = null;
            Exception ex = null;

            // Use the header name from options instead of a magic string.
            string correlationHeader = _options.Value.CorrelationHeaderName;
            string headerCorrelation = null;
            if (request.Headers.Contains(correlationHeader))
            {
                headerCorrelation = request.Headers.GetValues(correlationHeader).FirstOrDefault();
            }
            else
            {
                headerCorrelation = CorrelationContext.Current?.ParentId.ToString()
                                    ?? _correlationGenerator.Generate(1, 1).ToString();
                request.Headers.Add(correlationHeader, headerCorrelation);
            }

            // If no correlation scope exists, create one.
            if (CorrelationContext.Current == null)
                CorrelationContext.EnsureCorrelation(_correlationGenerator);

            // Generate a new child correlation id.
            string currentStepCorrelation = CorrelationContext.GetNextCorrelationId(_correlationGenerator);

            HttpResponseMessage response = null;
            try
            {
                response = await base.SendAsync(request, cancellationToken);

                if (response.Content != null)
                    resBody = await response.Content.ReadAsStringAsync();

                return response;
            }
            catch (Exception e)
            {
                ex = e;
                throw;
            }
            finally
            {
                sw.Stop();
                // Create the HTTP call log entry.
                var logEntry = new LinqHttpCallLog
                {
                    CorrelationId = currentStepCorrelation,
                    ParentCorrelationId = headerCorrelation,
                    Url = request.RequestUri.ToString(),
                    Method = request.Method.Method,
                    RequestBody = _masker.MaskRequest(reqBody),
                    ResponseBody = _masker.MaskResponse(resBody),
                    DurationMs = sw.ElapsedMilliseconds,
                    Exception = ex?.ToString(),
                    IsException = ex != null,
                    CreatedAt = DateTime.UtcNow,
                    IsInternal = false,
                    // Optionally, set extra analytic fields:
                    UserAgent = request.Headers.UserAgent.ToString(),
                    //ClientIP = request.ip, // Extension method to get IP from request
                    LogType = LogType.Outgoing // for example, distinguish outgoing HTTP calls
                };

                // Optionally, skip logging view (HTML) content if disabled.
                if (!_options.Value.LogViewContent && logEntry.Url.EndsWith(".html"))
                {
                    // Skip or mask view content
                    logEntry.ResponseBody = "[HTML content not logged]";
                }

                await _logger.LogAsync(logEntry);
            }
        }
    }


}

