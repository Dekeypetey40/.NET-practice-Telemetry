using Microsoft.AspNetCore.Mvc;
using Telemetry.Application.Contracts;

namespace Telemetry.Api.Controllers;

[ApiController]
[Route("runs")]
public class SupportController : ControllerBase
{
    private readonly ISupportBundleService _supportBundleService;

    public SupportController(ISupportBundleService supportBundleService) => _supportBundleService = supportBundleService;

    /// <summary>Maximum log entries allowed in a support bundle to prevent DoS.</summary>
    public const int MaxSupportBundleLogEntries = 1000;

    [HttpPost("{id:guid}/support-bundle")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSupportBundle(Guid id, [FromQuery] int lastLogEntries = 100, CancellationToken cancellationToken = default)
    {
        if (lastLogEntries < 1 || lastLogEntries > MaxSupportBundleLogEntries)
            return BadRequest(new { error = $"lastLogEntries must be between 1 and {MaxSupportBundleLogEntries}." });

        var stream = await _supportBundleService.CreateBundleForRunAsync(id, lastLogEntries, cancellationToken);
        return File(stream, "application/zip", $"support-bundle-{id:N}.zip");
    }
}
