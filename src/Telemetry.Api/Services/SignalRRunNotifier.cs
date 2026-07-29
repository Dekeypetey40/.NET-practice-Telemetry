using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<SignalRRunNotifier> _logger;

    public SignalRRunNotifier(IHubContext<RunHub> hubContext, ILogger<SignalRRunNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyRunChangedAsync(RunResponse run, CancellationToken cancellationToken = default)
    {
        // Post-commit notifications must be best-effort:
        // - do not turn a successful state transition into a 500
        // - do not let request cancellation abort notification delivery
        try
        {
            await _hubContext.Clients.All.SendAsync("RunChanged", run, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR notification failed for run {RunId}", run.Id);
        }
    }
}
