namespace Telemetry.Application.Contracts;

public interface ISupportBundleService
{
    Task<Stream> CreateBundleForRunAsync(Guid runId, int lastLogEntriesCount = 100, CancellationToken cancellationToken = default);
}
