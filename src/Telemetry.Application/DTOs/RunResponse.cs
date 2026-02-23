namespace Telemetry.Application.DTOs;

public record RunResponse(
    Guid Id,
    Guid InstrumentId,
    string SampleId,
    string MethodMetadataJson,
    string CurrentState,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? Actor,
    string? CorrelationId);
