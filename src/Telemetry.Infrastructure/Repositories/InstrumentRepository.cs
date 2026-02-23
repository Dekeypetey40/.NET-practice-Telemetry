using Microsoft.EntityFrameworkCore;
using Telemetry.Application.Contracts;
using Telemetry.Domain.Entities;
using Telemetry.Infrastructure.Data;

namespace Telemetry.Infrastructure.Repositories;

public class InstrumentRepository : IInstrumentRepository
{
    private readonly TelemetryDbContext _db;

    public InstrumentRepository(TelemetryDbContext db) => _db = db;

    public async Task<Instrument?> GetByIdAsync(Guid id, bool includeAlarms = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Instruments.AsQueryable();
        if (includeAlarms)
            query = query.Include(i => i.Alarms);
        return await query.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Instrument> AddAsync(Instrument instrument, CancellationToken cancellationToken = default)
    {
        await _db.Instruments.AddAsync(instrument, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return instrument;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}
