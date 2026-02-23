using Microsoft.AspNetCore.Mvc;
using Telemetry.Api.Middleware;
using Telemetry.Application.DTOs;
using Telemetry.Application.Services;

namespace Telemetry.Api.Controllers;

[ApiController]
[Route("runs")]
public class RunsController : ControllerBase
{
    private readonly IRunService _runService;

    public RunsController(IRunService runService) => _runService = runService;

    private string? CorrelationId => HttpContext.Items[CorrelationIdMiddleware.ItemKey] as string;

    [HttpPost]
    [ProducesResponseType(typeof(RunResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RunResponse>> Create([FromBody] CreateRunRequest request, CancellationToken cancellationToken)
    {
        var result = await _runService.CreateAsync(request, CorrelationId, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/queue")]
    [ProducesResponseType(typeof(RunResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RunResponse>> Queue(Guid id, [FromQuery] string? actor, CancellationToken cancellationToken)
    {
        var result = await _runService.QueueAsync(id, actor, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(RunResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RunResponse>> Start(Guid id, [FromQuery] string? actor, CancellationToken cancellationToken)
    {
        var result = await _runService.StartAsync(id, actor, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(RunResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RunResponse>> Cancel(Guid id, [FromQuery] string? actor, CancellationToken cancellationToken)
    {
        var result = await _runService.CancelAsync(id, actor, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RunResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RunResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _runService.GetByIdAsync(id, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/timeline")]
    [ProducesResponseType(typeof(RunTimelineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RunTimelineResponse>> GetTimeline(Guid id, CancellationToken cancellationToken)
    {
        var result = await _runService.GetTimelineAsync(id, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }
}
