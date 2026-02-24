using System.Diagnostics;

namespace LinkVault.Api.Middleware;

public sealed class CorrelationContextMiddleware
{
    public const string CorrelationHeader = "X-Correlation-ID";
    public const string CorrelationItemKey = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationContextMiddleware> _logger;

    public CorrelationContextMiddleware(RequestDelegate next, ILogger<CorrelationContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var incomingCorrelationId = context.Request.Headers[CorrelationHeader].FirstOrDefault();
        var correlationId = string.IsNullOrWhiteSpace(incomingCorrelationId)
            ? (Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier)
            : incomingCorrelationId;

        context.Items[CorrelationItemKey] = correlationId;
        context.Response.Headers[CorrelationHeader] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["TraceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier
        }))
        {
            await _next(context);
        }
    }
}
