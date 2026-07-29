using Microsoft.AspNetCore.SignalR;

namespace Telemetry.Api.Hubs;

/// <summary>
/// SignalR hub for real-time run state change notifications.
/// Clients connect to /hubs/runs and receive "RunChanged" messages
/// whenever a run transitions to a new state.
/// </summary>
public class RunHub : Hub
{
}
