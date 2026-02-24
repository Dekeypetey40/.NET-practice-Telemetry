using Telemetry.Domain.Enums;
using Telemetry.Domain.StateMachine;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Domain.Entities;

public class Run
{
    public Guid Id { get; private set; }
    public Guid InstrumentId { get; private set; }
    public string SampleId { get; private set; } = string.Empty;
    public string MethodMetadataJson { get; private set; } = "{}";
    public RunState CurrentState { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Actor { get; private set; }
    public string? CorrelationId { get; private set; }

    private readonly List<RunEvent> _events = new();
    public IReadOnlyCollection<RunEvent> Events => _events.AsReadOnly();

    private Run() { }

    public static Run Create(Guid instrumentId, SampleId sampleId, MethodMetadata? methodMetadata = null, string? correlationId = null)
    {
        return new Run
        {
            Id = Guid.NewGuid(),
            InstrumentId = instrumentId,
            SampleId = sampleId.Value,
            MethodMetadataJson = methodMetadata is null ? "{}" : System.Text.Json.JsonSerializer.Serialize(new { methodMetadata.MethodName, methodMetadata.Version, methodMetadata.Parameters }),
            CurrentState = RunState.Created,
            CreatedAt = DateTime.UtcNow,
            CorrelationId = correlationId
        };
    }

    public void RecordEvent(string eventType, string? data = null, string? actor = null)
    {
        var evt = RunEvent.Create(Id, eventType, data, actor ?? Actor, CorrelationId);
        _events.Add(evt);
    }

    public void SetQueued(string? actor = null)
    {
        if (!RunStateMachine.CanQueue(CurrentState))
            throw new InvalidOperationException($"Run is in state {CurrentState}; cannot queue. Only Created runs can be queued.");
        CurrentState = RunState.Queued;
        Actor = actor ?? Actor;
        RecordEvent("StateTransition", "Created→Queued", Actor);
    }

    public void SetRunning(string? actor = null)
    {
        if (!RunStateMachine.CanStart(CurrentState))
            throw new InvalidOperationException($"Run is in state {CurrentState}; cannot start. Only Queued runs can be started.");
        CurrentState = RunState.Running;
        StartedAt = DateTime.UtcNow;
        Actor = actor ?? Actor;
        RecordEvent("StateTransition", "Queued→Running", Actor);
    }

    public void SetCompleted(string? actor = null)
    {
        if (!RunStateMachine.CanComplete(CurrentState))
            throw new InvalidOperationException($"Run is in state {CurrentState}; cannot complete. Only Running runs can be completed.");
        CurrentState = RunState.Completed;
        CompletedAt = DateTime.UtcNow;
        Actor = actor ?? Actor;
        RecordEvent("StateTransition", "Running→Completed", Actor);
    }

    public void SetFailed(string? actor = null)
    {
        if (!RunStateMachine.CanFail(CurrentState))
            throw new InvalidOperationException($"Run is in state {CurrentState}; cannot fail. Only Running runs can be failed.");
        CurrentState = RunState.Failed;
        CompletedAt = DateTime.UtcNow;
        Actor = actor ?? Actor;
        RecordEvent("StateTransition", "Running→Failed", Actor);
    }

    public void SetCanceled(string? actor = null)
    {
        if (!RunStateMachine.CanCancel(CurrentState))
            throw new InvalidOperationException($"Run is in state {CurrentState}; cannot cancel. Only Created, Queued, or Running runs can be canceled.");
        var from = CurrentState.ToString();
        CurrentState = RunState.Canceled;
        CompletedAt = DateTime.UtcNow;
        Actor = actor ?? Actor;
        RecordEvent("StateTransition", $"{from}→Canceled", Actor);
    }
}
