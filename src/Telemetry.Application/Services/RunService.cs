using Telemetry.Application.Contracts;
using Telemetry.Application.DTOs;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Enums;
using Telemetry.Domain.StateMachine;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Application.Services;

public class RunService : IRunService
{
    private readonly IRunRepository _runRepository;
    private readonly IInstrumentRepository _instrumentRepository;

    public RunService(IRunRepository runRepository, IInstrumentRepository instrumentRepository)
    {
        _runRepository = runRepository;
        _instrumentRepository = instrumentRepository;
    }

    public async Task<RunResponse> CreateAsync(CreateRunRequest request, string? correlationId = null, CancellationToken cancellationToken = default)
    {
        var instrument = await _instrumentRepository.GetByIdAsync(request.InstrumentId, cancellationToken: cancellationToken);
        if (instrument == null)
            throw new KeyNotFoundException($"Instrument {request.InstrumentId} not found.");

        var sampleId = SampleId.Create(request.SampleId);
        var methodMetadata = string.IsNullOrWhiteSpace(request.MethodName)
            ? null
            : new MethodMetadata(request.MethodName, request.MethodVersion, request.Parameters);

        var run = Run.Create(request.InstrumentId, sampleId, methodMetadata, correlationId);
        await _runRepository.AddAsync(run, cancellationToken);
        return ToResponse(run);
    }

    public async Task<RunResponse> QueueAsync(Guid runId, string? actor = null, CancellationToken cancellationToken = default)
    {
        var run = await _runRepository.GetByIdAsync(runId, includeEvents: true, cancellationToken);
        if (run == null)
            throw new KeyNotFoundException($"Run {runId} not found.");
        if (!RunStateMachine.CanQueue(run.CurrentState))
            throw new InvalidOperationException($"Run is in state {run.CurrentState}; cannot queue. Only Created runs can be queued.");
        run.SetQueued(actor);
        await _runRepository.SaveChangesAsync(cancellationToken);
        return ToResponse(run);
    }

    public async Task<RunResponse> StartAsync(Guid runId, string? actor = null, CancellationToken cancellationToken = default)
    {
        var run = await _runRepository.GetByIdAsync(runId, includeEvents: true, cancellationToken);
        if (run == null)
            throw new KeyNotFoundException($"Run {runId} not found.");
        if (!RunStateMachine.CanStart(run.CurrentState))
            throw new InvalidOperationException($"Run is in state {run.CurrentState}; cannot start. Only Queued runs can be started.");
        run.SetRunning(actor);
        await _runRepository.SaveChangesAsync(cancellationToken);
        return ToResponse(run);
    }

    public async Task<RunResponse> CancelAsync(Guid runId, string? actor = null, CancellationToken cancellationToken = default)
    {
        var run = await _runRepository.GetByIdAsync(runId, includeEvents: true, cancellationToken);
        if (run == null)
            throw new KeyNotFoundException($"Run {runId} not found.");
        if (!RunStateMachine.CanCancel(run.CurrentState))
            throw new InvalidOperationException($"Run is in state {run.CurrentState}; cannot cancel. Only Created, Queued, or Running runs can be canceled.");
        run.SetCanceled(actor);
        await _runRepository.SaveChangesAsync(cancellationToken);
        return ToResponse(run);
    }

    public async Task<RunResponse?> GetByIdAsync(Guid runId, CancellationToken cancellationToken = default)
    {
        var run = await _runRepository.GetByIdAsync(runId, includeEvents: false, cancellationToken);
        return run == null ? null : ToResponse(run);
    }

    public async Task<RunTimelineResponse?> GetTimelineAsync(Guid runId, CancellationToken cancellationToken = default)
    {
        var run = await _runRepository.GetByIdAsync(runId, includeEvents: false, cancellationToken);
        if (run == null)
            return null;
        var events = await _runRepository.GetTimelineAsync(runId, cancellationToken);
        return new RunTimelineResponse(
            runId,
            events.Select(e => new RunTimelineEventResponse(e.Id, e.EventType, e.Timestamp, e.Data, e.Actor, e.CorrelationId)).ToList());
    }

    private static RunResponse ToResponse(Run run) => new(
        run.Id,
        run.InstrumentId,
        run.SampleId,
        run.MethodMetadataJson,
        run.CurrentState.ToString(),
        run.CreatedAt,
        run.StartedAt,
        run.CompletedAt,
        run.Actor,
        run.CorrelationId);
}
