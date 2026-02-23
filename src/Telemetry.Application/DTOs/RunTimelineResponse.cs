namespace Telemetry.Application.DTOs;

public record RunTimelineEventResponse(Guid Id, string EventType, DateTime Timestamp, string? Data, string? Actor, string? CorrelationId);

public record RunTimelineResponse(Guid RunId, IReadOnlyList<RunTimelineEventResponse> Events);
