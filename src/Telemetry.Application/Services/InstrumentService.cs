using Telemetry.Application.Contracts;
using Telemetry.Application.DTOs;
using Telemetry.Domain.Entities;

namespace Telemetry.Application.Services;

public class InstrumentService : IInstrumentService
{
    private readonly IInstrumentRepository _instrumentRepository;

    public InstrumentService(IInstrumentRepository instrumentRepository) => _instrumentRepository = instrumentRepository;

    public async Task<InstrumentHealthResponse> CreateAsync(CreateInstrumentRequest request, CancellationToken cancellationToken = default)
    {
        var instrument = Instrument.Create(request.Name, request.Type, request.SerialNumber);
        await _instrumentRepository.AddAsync(instrument, cancellationToken);
        return ToHealthResponse(instrument);
    }

    public async Task<InstrumentHealthResponse?> GetHealthAsync(Guid instrumentId, CancellationToken cancellationToken = default)
    {
        var instrument = await _instrumentRepository.GetByIdAsync(instrumentId, includeAlarms: true, cancellationToken);
        return instrument == null ? null : ToHealthResponse(instrument);
    }

    public async Task<IReadOnlyList<InstrumentResponse>> GetRecentAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        var instruments = await _instrumentRepository.GetRecentAsync(limit, cancellationToken);
        return instruments.Select(ToResponse).ToList();
    }

    private static InstrumentResponse ToResponse(Instrument i) => new(
        i.Id,
        i.Name,
        i.Type,
        i.SerialNumber,
        i.Status,
        i.CreatedAt,
        i.LastHealthCheck);

    private static InstrumentHealthResponse ToHealthResponse(Instrument i) => new(
        i.Id,
        i.Name,
        i.Status,
        i.LastHealthCheck,
        i.Alarms.Select(a => new AlarmResponse(a.Id, a.Severity, a.Message, a.RaisedAt, a.AcknowledgedAt)).ToList());
}
