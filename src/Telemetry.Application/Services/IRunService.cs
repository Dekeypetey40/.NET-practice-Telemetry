using Telemetry.Application.DTOs;

namespace Telemetry.Application.Services;

public interface IRunService
{
    Task<RunResponse> CreateAsync(CreateRunRequest request, string? correlationId = null, CancellationToken cancellationToken = default);
    Task<RunResponse> QueueAsync(Guid runId, string? actor = null, CancellationToken cancellationToken = default);
    Task<RunResponse> StartAsync(Guid runId, string? actor = null, CancellationToken cancellationToken = default);
    Task<RunResponse> CancelAsync(Guid runId, string? actor = null, CancellationToken cancellationToken = default);
    Task<RunResponse> CompleteAsync(Guid runId, string? actor = null, CancellationToken cancellationToken = default);
    Task<RunResponse> FailAsync(Guid runId, string? actor = null, CancellationToken cancellationToken = default);
    Task<RunResponse?> GetByIdAsync(Guid runId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RunResponse>> GetRecentAsync(int limit = 50, CancellationToken cancellationToken = default);
    Task<RunTimelineResponse?> GetTimelineAsync(Guid runId, CancellationToken cancellationToken = default);
}
