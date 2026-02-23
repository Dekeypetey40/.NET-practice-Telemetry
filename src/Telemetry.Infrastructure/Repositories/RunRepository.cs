using Microsoft.EntityFrameworkCore;
using Telemetry.Application.Contracts;
using Telemetry.Domain.Entities;
using Telemetry.Infrastructure.Data;

namespace Telemetry.Infrastructure.Repositories;

public class RunRepository : IRunRepository
{
    private readonly TelemetryDbContext _db;

    public RunRepository(TelemetryDbContext db) => _db = db;

    public async Task<Run?> GetByIdAsync(Guid id, bool includeEvents = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Runs.AsQueryable();
        if (includeEvents)
            query = query.Include(r => r.Events);
        return await query.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Run> AddAsync(Run run, CancellationToken cancellationToken = default)
    {
        await _db.Runs.AddAsync(run, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return run;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<RunEvent>> GetTimelineAsync(Guid runId, CancellationToken cancellationToken = default)
    {
        return await _db.RunEvents
            .Where(e => e.RunId == runId)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(cancellationToken);
    }
}
