using Telemetry.Application.DTOs;

namespace Telemetry.Application.Contracts;

/// <summary>
/// Publishes run state changes to connected clients.
/// Implemented in the API layer (e.g. via SignalR) to keep the Application layer transport-agnostic.
/// </summary>
public interface IRunNotifier
{
    Task NotifyRunChangedAsync(RunResponse run, CancellationToken cancellationToken = default);
}
