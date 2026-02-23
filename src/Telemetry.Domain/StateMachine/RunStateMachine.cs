using Telemetry.Domain.Enums;

namespace Telemetry.Domain.StateMachine;

public static class RunStateMachine
{
    private static readonly IReadOnlyDictionary<RunState, IReadOnlySet<RunState>> AllowedTransitions = new Dictionary<RunState, IReadOnlySet<RunState>>
    {
        [RunState.Created] = new HashSet<RunState> { RunState.Queued, RunState.Canceled },
        [RunState.Queued] = new HashSet<RunState> { RunState.Running, RunState.Canceled },
        [RunState.Running] = new HashSet<RunState> { RunState.Completed, RunState.Failed, RunState.Canceled },
        [RunState.Completed] = new HashSet<RunState>(),
        [RunState.Failed] = new HashSet<RunState>(),
        [RunState.Canceled] = new HashSet<RunState>()
    };

    public static bool CanTransitionTo(RunState current, RunState target)
    {
        return AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Contains(target);
    }

    public static bool CanQueue(RunState current) => CanTransitionTo(current, RunState.Queued);
    public static bool CanStart(RunState current) => CanTransitionTo(current, RunState.Running);
    public static bool CanComplete(RunState current) => CanTransitionTo(current, RunState.Completed);
    public static bool CanFail(RunState current) => CanTransitionTo(current, RunState.Failed);
    public static bool CanCancel(RunState current) => CanTransitionTo(current, RunState.Canceled);

    public static bool IsTerminal(RunState state) =>
        state == RunState.Completed || state == RunState.Failed || state == RunState.Canceled;
}
