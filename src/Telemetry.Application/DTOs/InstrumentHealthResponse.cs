namespace Telemetry.Application.DTOs;

public record AlarmResponse(Guid Id, string Severity, string Message, DateTime RaisedAt, DateTime? AcknowledgedAt);

public record InstrumentHealthResponse(
    Guid InstrumentId,
    string Name,
    string Status,
    DateTime? LastHealthCheck,
    IReadOnlyList<AlarmResponse> Alarms);
