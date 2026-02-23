using FluentAssertions;
using Telemetry.Domain.Enums;
using Telemetry.Domain.StateMachine;
using Xunit;

namespace Telemetry.UnitTests.StateMachine;

public class RunStateMachineTests
{
    [Theory]
    [InlineData(RunState.Created, RunState.Queued, true)]
    [InlineData(RunState.Created, RunState.Canceled, true)]
    [InlineData(RunState.Created, RunState.Running, false)]
    [InlineData(RunState.Queued, RunState.Running, true)]
    [InlineData(RunState.Queued, RunState.Canceled, true)]
    [InlineData(RunState.Queued, RunState.Created, false)]
    [InlineData(RunState.Running, RunState.Completed, true)]
    [InlineData(RunState.Running, RunState.Failed, true)]
    [InlineData(RunState.Running, RunState.Canceled, true)]
    [InlineData(RunState.Running, RunState.Queued, false)]
    [InlineData(RunState.Completed, RunState.Running, false)]
    [InlineData(RunState.Failed, RunState.Running, false)]
    [InlineData(RunState.Canceled, RunState.Queued, false)]
    public void CanTransitionTo_ReturnsExpected(RunState current, RunState target, bool expected)
    {
        RunStateMachine.CanTransitionTo(current, target).Should().Be(expected);
    }

    [Fact]
    public void CanQueue_OnlyFromCreated()
    {
        RunStateMachine.CanQueue(RunState.Created).Should().BeTrue();
        RunStateMachine.CanQueue(RunState.Queued).Should().BeFalse();
        RunStateMachine.CanQueue(RunState.Running).Should().BeFalse();
        RunStateMachine.CanQueue(RunState.Completed).Should().BeFalse();
    }

    [Fact]
    public void CanStart_OnlyFromQueued()
    {
        RunStateMachine.CanStart(RunState.Queued).Should().BeTrue();
        RunStateMachine.CanStart(RunState.Created).Should().BeFalse();
        RunStateMachine.CanStart(RunState.Running).Should().BeFalse();
    }

    [Fact]
    public void CanCancel_FromCreatedQueuedOrRunning()
    {
        RunStateMachine.CanCancel(RunState.Created).Should().BeTrue();
        RunStateMachine.CanCancel(RunState.Queued).Should().BeTrue();
        RunStateMachine.CanCancel(RunState.Running).Should().BeTrue();
        RunStateMachine.CanCancel(RunState.Completed).Should().BeFalse();
        RunStateMachine.CanCancel(RunState.Failed).Should().BeFalse();
        RunStateMachine.CanCancel(RunState.Canceled).Should().BeFalse();
    }

    [Fact]
    public void IsTerminal_OnlyForCompletedFailedCanceled()
    {
        RunStateMachine.IsTerminal(RunState.Completed).Should().BeTrue();
        RunStateMachine.IsTerminal(RunState.Failed).Should().BeTrue();
        RunStateMachine.IsTerminal(RunState.Canceled).Should().BeTrue();
        RunStateMachine.IsTerminal(RunState.Created).Should().BeFalse();
        RunStateMachine.IsTerminal(RunState.Queued).Should().BeFalse();
        RunStateMachine.IsTerminal(RunState.Running).Should().BeFalse();
    }
}
