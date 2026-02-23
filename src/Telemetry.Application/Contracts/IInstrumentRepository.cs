using Telemetry.Domain.Entities;

namespace Telemetry.Application.Contracts;

public interface IInstrumentRepository
{
    Task<Instrument?> GetByIdAsync(Guid id, bool includeAlarms = false, CancellationToken cancellationToken = default);
    Task<Instrument> AddAsync(Instrument instrument, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
