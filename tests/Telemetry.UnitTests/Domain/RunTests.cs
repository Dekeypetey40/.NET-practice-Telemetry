using FluentAssertions;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Enums;
using Telemetry.Domain.StateMachine;
using Telemetry.Domain.ValueObjects;
using Xunit;

namespace Telemetry.UnitTests.Domain;

public class RunTests
{
    [Fact]
    public void Create_SetsCreatedState()
    {
        var instrumentId = Guid.NewGuid();
        var sampleId = SampleId.Create("S-001");
        var run = Run.Create(instrumentId, sampleId, new MethodMetadata("MethodA", "1.0"), "corr-1");

        run.InstrumentId.Should().Be(instrumentId);
        run.SampleId.Should().Be("S-001");
        run.CurrentState.Should().Be(RunState.Created);
        run.CorrelationId.Should().Be("corr-1");
        run.Events.Should().BeEmpty();
    }

    [Fact]
    public void SetQueued_TransitionsAndRecordsEvent()
    {
        var run = Run.Create(Guid.NewGuid(), SampleId.Create("S-1"));
        run.SetQueued("operator");

        run.CurrentState.Should().Be(RunState.Queued);
        run.Actor.Should().Be("operator");
        run.Events.Should().ContainSingle(e => e.EventType == "StateTransition" && e.Data == "Created→Queued");
    }

    [Fact]
    public void SetRunning_SetsStartedAt()
    {
        var run = Run.Create(Guid.NewGuid(), SampleId.Create("S-1"));
        run.SetQueued();
        run.SetRunning("system");

        run.CurrentState.Should().Be(RunState.Running);
        run.StartedAt.Should().NotBeNull();
        run.Events.Should().Contain(e => e.Data == "Queued→Running");
    }

    [Fact]
    public void SetCanceled_FromCreated_RecordsTransition()
    {
        var run = Run.Create(Guid.NewGuid(), SampleId.Create("S-1"));
        run.SetCanceled("user");

        run.CurrentState.Should().Be(RunState.Canceled);
        run.CompletedAt.Should().NotBeNull();
        run.Events.Should().Contain(e => e.Data == "Created→Canceled");
    }
}
