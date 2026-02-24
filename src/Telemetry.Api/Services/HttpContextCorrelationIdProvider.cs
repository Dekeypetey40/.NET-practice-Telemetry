using Telemetry.Api.Middleware;

namespace Telemetry.Api.Services;

/// <summary>Reads the correlation ID from the current HTTP context (set by CorrelationIdMiddleware).</summary>
public sealed class HttpContextCorrelationIdProvider : ICorrelationIdProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCorrelationIdProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCorrelationId() =>
        _httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.ItemKey] as string;
}
