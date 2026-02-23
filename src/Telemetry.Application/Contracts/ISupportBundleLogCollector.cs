namespace Telemetry.Application.Contracts;

public interface ISupportBundleLogCollector
{
    IReadOnlyList<string> GetRecentEntries(int count);
}
