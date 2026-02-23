using Microsoft.AspNetCore.Mvc;
using Telemetry.Application.DTOs;
using Telemetry.Application.Services;

namespace Telemetry.Api.Controllers;

[ApiController]
[Route("instruments")]
public class InstrumentsController : ControllerBase
{
    private readonly IInstrumentService _instrumentService;

    public InstrumentsController(IInstrumentService instrumentService) => _instrumentService = instrumentService;

    [HttpPost]
    [ProducesResponseType(typeof(InstrumentHealthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InstrumentHealthResponse>> Create([FromBody] CreateInstrumentRequest request, CancellationToken cancellationToken)
    {
        var result = await _instrumentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetHealth), new { id = result.InstrumentId }, result);
    }

    [HttpGet("{id:guid}/health")]
    [ProducesResponseType(typeof(InstrumentHealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InstrumentHealthResponse>> GetHealth(Guid id, CancellationToken cancellationToken)
    {
        var result = await _instrumentService.GetHealthAsync(id, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }
}
