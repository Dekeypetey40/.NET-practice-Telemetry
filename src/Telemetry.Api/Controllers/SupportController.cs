using Microsoft.AspNetCore.Mvc;
using Telemetry.Application.Contracts;

namespace Telemetry.Api.Controllers;

[ApiController]
[Route("runs")]
public class SupportController : ControllerBase
{
    private readonly ISupportBundleService _supportBundleService;

    public SupportController(ISupportBundleService supportBundleService) => _supportBundleService = supportBundleService;

    [HttpPost("{id:guid}/support-bundle")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateSupportBundle(Guid id, [FromQuery] int lastLogEntries = 100, CancellationToken cancellationToken = default)
    {
        var stream = await _supportBundleService.CreateBundleForRunAsync(id, lastLogEntries, cancellationToken);
        return File(stream, "application/zip", $"support-bundle-{id:N}.zip");
    }
}
