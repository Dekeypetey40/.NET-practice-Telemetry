namespace Telemetry.Domain.Enums;

public enum RunState
{
    Created = 0,
    Queued = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Canceled = 5
}
