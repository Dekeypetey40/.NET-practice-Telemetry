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

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureNewRunEventsTrackedAsync(cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("The run was modified by another request. Please refresh and retry.");
        }
    }

    private async Task EnsureNewRunEventsTrackedAsync(CancellationToken cancellationToken)
    {
        var modifiedRuns = _db.ChangeTracker.Entries<Run>()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();
        foreach (var run in modifiedRuns)
        {
            foreach (var evt in run.Events)
            {
                var entry = _db.Entry(evt);
                if (entry.State == EntityState.Detached)
                    await _db.RunEvents.AddAsync(evt, cancellationToken);
                else if (entry.State == EntityState.Modified)
                    entry.State = EntityState.Added;
            }
        }
    }

    public async Task<IReadOnlyList<RunEvent>> GetTimelineAsync(Guid runId, CancellationToken cancellationToken = default)
    {
        return await _db.RunEvents
            .Where(e => e.RunId == runId)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(cancellationToken);
    }
}
