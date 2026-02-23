namespace Telemetry.Application.DTOs;

public record CreateRunRequest(Guid InstrumentId, string SampleId, string? MethodName = null, string? MethodVersion = null, IReadOnlyDictionary<string, string>? Parameters = null);
