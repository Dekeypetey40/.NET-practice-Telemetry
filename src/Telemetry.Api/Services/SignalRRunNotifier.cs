using Microsoft.AspNetCore.SignalR;
using Telemetry.Api.Hubs;
using Telemetry.Application.Contracts;
using Telemetry.Application.DTOs;

namespace Telemetry.Api.Services;

/// <summary>
/// Broadcasts run state changes to all connected SignalR clients.
/// </summary>
public class SignalRRunNotifier : IRunNotifier
{
    private readonly IHubContext<RunHub> _hubContext;

    public SignalRRunNotifier(IHubContext<RunHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyRunChangedAsync(RunResponse run, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync("RunChanged", run, cancellationToken);
    }
}
