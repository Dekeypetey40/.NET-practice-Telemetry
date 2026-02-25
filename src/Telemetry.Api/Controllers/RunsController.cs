using Microsoft.AspNetCore.Mvc;
using Telemetry.Api.Services;
using Telemetry.Application.DTOs;
using Telemetry.Application.Services;

namespace Telemetry.Api.Controllers;

[ApiController]
[Route("runs")]
public class RunsController : ControllerBase
{
    private readonly IRunService _runService;
    private readonly ICorrelationIdProvider _correlationIdProvider;

    public RunsController(IRunService runService, ICorrelationIdProvider correlationIdProvider)
    {
        _runService = runService;
        _correlationIdProvider = correlationIdProvider;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RunResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RunResponse>> Create([FromBody] CreateRunRequest request, CancellationToken cancellationToken)
    {
        var result = await _runService.CreateAsync(request, _correlationIdProvider.GetCorrelationId(), cancellationToken);
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

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(RunResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RunResponse>> Complete(Guid id, [FromQuery] string? actor, CancellationToken cancellationToken)
    {
        var result = await _runService.CompleteAsync(id, actor, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/fail")]
    [ProducesResponseType(typeof(RunResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RunResponse>> Fail(Guid id, [FromQuery] string? actor, CancellationToken cancellationToken)
    {
        var result = await _runService.FailAsync(id, actor, cancellationToken);
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
