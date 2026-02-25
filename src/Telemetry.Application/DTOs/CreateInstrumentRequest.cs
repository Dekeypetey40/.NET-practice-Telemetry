using System.ComponentModel.DataAnnotations;

namespace Telemetry.Application.DTOs;

public record CreateInstrumentRequest(
    [Required, MinLength(1), MaxLength(256)] string Name,
    [Required, MinLength(1), MaxLength(128)] string Type,
    [MaxLength(128)] string? SerialNumber = null);
