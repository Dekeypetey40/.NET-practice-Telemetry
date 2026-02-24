using System.ComponentModel.DataAnnotations;

namespace Telemetry.Application.DTOs;

public record CreateInstrumentRequest(
    [property: Required, MinLength(1), MaxLength(256)] string Name,
    [property: Required, MinLength(1), MaxLength(128)] string Type,
    [MaxLength(128)] string? SerialNumber = null);
