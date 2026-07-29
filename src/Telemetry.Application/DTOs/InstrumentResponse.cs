namespace Telemetry.Application.DTOs;

public record InstrumentResponse(
    Guid Id,
    string Name,
    string Type,
    string? SerialNumber,
    string Status,
    DateTime CreatedAt,
    DateTime? LastHealthCheck);
