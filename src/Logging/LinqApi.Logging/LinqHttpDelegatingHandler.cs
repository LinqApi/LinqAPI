using LinqApi.Correlation;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace LinqApi.Logging
{
    /// <summary>
    /// Delegating handler that logs HTTP requests/responses as "Incoming" log entries.
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
            _logger = logger;
            _masker = masker;
            _correlationGenerator = correlationGenerator;
            _options = options;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            string reqBody = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken) : null;
            string resBody = null;
            Exception ex = null;

            // Header ismi options'tan alınıyor.
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

            // Eğer correlation scope mevcut değilse oluştur.
            if (CorrelationContext.Current == null)
                CorrelationContext.EnsureCorrelation(_correlationGenerator);

            // Yeni bir child correlation id oluştur.

            HttpResponseMessage response = null;
            try
            {
                response = await base.SendAsync(request, cancellationToken);

                if (response.Content != null)
                    resBody = await response.Content.ReadAsStringAsync(cancellationToken);

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
                // HTTP call log entry oluşturuluyor.
                var logEntry = new LinqHttpCallLog
                {
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
                    UserAgent = request.Headers.UserAgent?.ToString() ?? string.Empty,
                    // ClientIP gibi ek alanlar eklenebilir, örneğin extension method ile request'ten IP alınabilir.
                };

                // Eğer view (HTML) içeriği loglanmayacaksa filtreleme yapılabilir.
                if (!_options.Value.LogViewContent && logEntry.Url.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                {
                    logEntry.ResponseBody = "[HTML content not logged]";
                }

                await _logger.LogAsync(logEntry, cancellationToken);
            }
        }
    }

}