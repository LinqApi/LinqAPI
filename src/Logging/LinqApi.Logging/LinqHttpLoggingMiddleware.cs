
using LinqApi.Correlation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;

namespace LinqApi.Core
{
    public class LinqHttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILinqPayloadMasker _masker;
        private readonly ICorrelationIdGenerator _correlationGenerator;
        private readonly IOptions<LinqLoggingOptions> _options;
        private readonly IServiceScopeFactory _scopeFactory;

        public LinqHttpLoggingMiddleware(
            RequestDelegate next,
            ILinqPayloadMasker masker,
            ICorrelationIdGenerator correlationGenerator,
            IOptions<LinqLoggingOptions> options,
            IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _masker = masker;
            _correlationGenerator = correlationGenerator;
            _options = options;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            string reqBody = string.Empty;

            // Enable buffering to read the request body.
            context.Request.EnableBuffering();
            if (context.Request.ContentLength.GetValueOrDefault() > 0)
            {
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    reqBody = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }
            }

            // Correlation ID ayarı.
            string correlationHeader = _options.Value.CorrelationHeaderName;
            string headerCorrelation = context.Request.Headers[correlationHeader].FirstOrDefault();
            if (string.IsNullOrEmpty(headerCorrelation))
            {
                headerCorrelation = CorrelationContext.Current?.ParentId.ToString()
                                    ?? _correlationGenerator.Generate(1, 1).ToString();
                context.Request.Headers[correlationHeader] = headerCorrelation;
            }
            if (CorrelationContext.Current == null)
                CorrelationContext.EnsureCorrelation(_correlationGenerator);

           
            var routeValues = context.GetRouteData();

            string controller = string.Empty;
            string action = string.Empty;

            if (routeValues != null)
            {
                if (routeValues.Values.TryGetValue("controller", out var ctrl) && ctrl != null)
                {
                    controller = ctrl.ToString();
                }

                if (routeValues.Values.TryGetValue("action", out var act) && act != null)
                {
                    action = act.ToString();
                }
            }

            // Response body'yi yakalamak için stream'i değiştir.
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            Exception exception = null;
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                sw.Stop();
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                string resBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                // Log entry oluştururken null değerler boş string ile değiştirilir.
                var logEntry = new LinqHttpCallLog
                {
                    ParentCorrelationId = headerCorrelation ?? string.Empty,
                    Url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
                    Method = context.Request.Method ?? string.Empty,
                    RequestBody = _masker.MaskRequest(reqBody) ?? string.Empty,
                    ResponseBody = _masker.MaskResponse(resBody) ?? string.Empty,
                    DurationMs = sw.ElapsedMilliseconds,
                    Exception = exception?.ToString() ?? string.Empty,
                    IsException = exception != null,
                    CreatedAt = DateTime.Now,
                    // Inbound çağrı için CallType ayarlanabilir.
                    UserAgent = context.Request.Headers["User-Agent"].ToString() ?? string.Empty,
                    Controller = controller,
                    Action = action
                };

                if (!_options.Value.LogViewContent && logEntry.Url.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                {
                    logEntry.ResponseBody = "[HTML content not logged]";
                }

                // Yeni scope ile ILinqLogger çözümlemesi.
                using (var scope = _scopeFactory.CreateScope())
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILinqLogger>();
                    await logger.LogAsync(logEntry, context.RequestAborted);
                }

                await responseBodyStream.CopyToAsync(originalBodyStream);
            }
        }
    }
}