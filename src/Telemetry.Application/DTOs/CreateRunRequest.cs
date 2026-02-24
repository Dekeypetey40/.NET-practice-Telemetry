using System.ComponentModel.DataAnnotations;

namespace Telemetry.Application.DTOs;

public record CreateRunRequest(
    Guid InstrumentId,
    [property: Required, MinLength(1), MaxLength(256)] string SampleId,
    [property: MaxLength(128)] string? MethodName = null,
    [property: MaxLength(32)] string? MethodVersion = null,
    IReadOnlyDictionary<string, string>? Parameters = null);
