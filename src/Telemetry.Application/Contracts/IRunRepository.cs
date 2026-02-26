using Telemetry.Domain.Entities;

namespace Telemetry.Application.Contracts;

public interface IRunRepository
{
    Task<Run?> GetByIdAsync(Guid id, bool includeEvents = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Run>> GetRecentAsync(int limit, CancellationToken cancellationToken = default);
    Task<Run> AddAsync(Run run, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RunEvent>> GetTimelineAsync(Guid runId, CancellationToken cancellationToken = default);
}
