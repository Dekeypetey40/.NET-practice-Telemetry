namespace Telemetry.Application.DTOs;

public record CreateInstrumentRequest(string Name, string Type, string? SerialNumber = null);
