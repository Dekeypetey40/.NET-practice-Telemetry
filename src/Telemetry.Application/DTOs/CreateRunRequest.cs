using System.ComponentModel.DataAnnotations;

namespace Telemetry.Application.DTOs;

public record CreateRunRequest(
    Guid InstrumentId,
    [Required, MinLength(1), MaxLength(256)] string SampleId,
    [MaxLength(128)] string? MethodName = null,
    [MaxLength(32)] string? MethodVersion = null,
    IReadOnlyDictionary<string, string>? Parameters = null);
