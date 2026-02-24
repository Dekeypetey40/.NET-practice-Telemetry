namespace Telemetry.Api.Services;

/// <summary>Provides the current request's correlation ID (set by CorrelationIdMiddleware).</summary>
public interface ICorrelationIdProvider
{
    string? GetCorrelationId();
}
