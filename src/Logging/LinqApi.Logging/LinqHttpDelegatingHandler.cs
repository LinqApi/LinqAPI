using LinqApi.Correlation;
using LinqApi.Logging;
using Microsoft.Extensions.Options;

using System.Diagnostics;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="LinqHttpDelegatingHandler"/> class.
    /// </summary>
    /// <param name="logger">The logging service for persisting HTTP logs.</param>
    /// <param name="masker">Payload masker used to obfuscate sensitive request/response data.</param>
    /// <param name="correlationGenerator">Correlation ID generator for request tracing.</param>
    /// <param name="options">Logging configuration options.</param>
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

    /// <summary>
    /// Sends an HTTP request asynchronously and logs the request/response details for observability.
    /// </summary>
    /// <param name="request">The outgoing HTTP request message.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        string reqBody = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken) : null;
        string resBody = null;
        Exception ex = null;

        // Correlation header configuration
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

        // Ensure correlation context exists
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

            var logEntry = new LinqHttpCallLog
            {
                ParentCorrelationId = headerCorrelation,
                Url = request.RequestUri?.ToString(),
                Method = request.Method.Method,
                RequestBody = _masker.MaskRequest(reqBody),
                ResponseBody = _masker.MaskResponse(resBody),
                DurationMs = sw.ElapsedMilliseconds,
                Exception = ex?.ToString(),
                IsException = ex != null,
                CreatedAt = DateTime.UtcNow,
                IsInternal = false,
                UserAgent = request.Headers.UserAgent?.ToString() ?? string.Empty
            };

            // Filter view content if configured
            if (!_options.Value.LogViewContent &&
                logEntry.Url?.EndsWith(".html", StringComparison.OrdinalIgnoreCase) == true)
            {
                logEntry.ResponseBody = "[HTML content not logged]";
            }

            await _logger.LogAsync(logEntry, cancellationToken);
        }
    }
}