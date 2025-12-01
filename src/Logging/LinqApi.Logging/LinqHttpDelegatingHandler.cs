using LinqApi.Correlation;
using Microsoft.Extensions.Options;

using System.Diagnostics;

namespace LinqApi.Core
{


    /// <summary>
    /// A delegating handler that logs outbound HTTP requests and their corresponding responses,
    /// enriched with correlation ID and payload masking support.
    /// This is useful for observability, diagnostics, and request tracing across distributed services.
    /// </summary>
    public class LinqHttpDelegatingHandler : DelegatingHandler
    {
        private readonly ILinqLogger _logger;
        private readonly ILinqPayloadMasker _masker;
        private readonly ICorrelationIdGenerator _correlationGenerator;
        private readonly IOptions<LinqLoggingOptions> _options;

        public LinqHttpDelegatingHandler(
            ILinqLogger logger,
            ILinqPayloadMasker masker,
            ICorrelationIdGenerator correlationGenerator,
            IOptions<LinqLoggingOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _masker = masker ?? throw new ArgumentNullException(nameof(masker));
            _correlationGenerator = correlationGenerator ?? throw new ArgumentNullException(nameof(correlationGenerator));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            string reqBody = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken) : string.Empty;
            string resBody = string.Empty;
            Exception ex = null;

            // Correlation header ayarı.
            string correlationHeader = _options.Value.CorrelationHeaderName;
            string headerCorrelation = request.Headers.Contains(correlationHeader)
                ? request.Headers.GetValues(correlationHeader).FirstOrDefault() ?? string.Empty
                : (_correlationGenerator.Generate(1, 1).ToString() ?? string.Empty);

            if (!request.Headers.Contains(correlationHeader))
            {
                request.Headers.Add(correlationHeader, headerCorrelation);
            }

            // Ensure correlation context exists.
            if (CorrelationContext.Current == null)
            {
                CorrelationContext.EnsureCorrelation(_correlationGenerator);
            }

            HttpResponseMessage response = null;
            try
            {
                response = await base.SendAsync(request, cancellationToken);

                if (response.Content != null)
                {
                    resBody = await response.Content.ReadAsStringAsync(cancellationToken);
                }
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
                OutboundHttpCallError logEntry = null;
                var url = request.RequestUri?.ToString() ?? string.Empty;
                var maskedRequest = _masker.MaskRequest(reqBody) ?? string.Empty;
                string maskedResponse = _masker.MaskResponse(resBody) ?? string.Empty;
                if (ex != null)
                {

                    logEntry = new OutboundHttpCallError
                    {
                        ParentCorrelationId = headerCorrelation,
                        Url = request.RequestUri?.ToString() ?? string.Empty,
                        Method = request.Method.Method ?? string.Empty,
                        RequestBody = maskedRequest,
                        ResponseBody = maskedResponse,
                        DurationMs = sw.ElapsedMilliseconds,
                        Exception = ex?.ToString() ?? string.Empty,
                        IsException = ex != null,
                        CreatedAt = DateTime.Now,
                        UserAgent = request.Headers.UserAgent?.ToString() ?? string.Empty,
                        Controller = string.Empty, // Outbound çağrılarda controller/action yoktur.
                        Action = string.Empty
                    };
                }


                if (!_options.Value.LogViewContent &&
                    url.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                {
                    logEntry.ResponseBody = "[HTML content not logged]";
                }

                await _logger.LogAsync(logEntry, cancellationToken);
            }
        }
    }
}