using Telemetry.Application.DTOs;

namespace Telemetry.Application.Services;

public interface IInstrumentService
{
    Task<InstrumentHealthResponse> CreateAsync(CreateInstrumentRequest request, CancellationToken cancellationToken = default);
    Task<InstrumentHealthResponse?> GetHealthAsync(Guid instrumentId, CancellationToken cancellationToken = default);
}
