using System.Collections.Concurrent;
using Telemetry.Application.Contracts;

namespace Telemetry.Infrastructure.Services;

/// <summary>
/// In-memory ring buffer of recent log entries for support bundles.
/// Entries are appended by a Serilog sink (or similar); this class only stores and returns them.
/// Suitable for dev/single-instance; use an external sink for production at scale.
/// </summary>
public sealed class InMemoryRingBufferLogCollector : ISupportBundleLogCollector
{
    private readonly ConcurrentQueue<string> _entries = new();
    private readonly int _maxEntries;
    private int _count;

    public InMemoryRingBufferLogCollector(int maxEntries = 2000)
    {
        _maxEntries = Math.Clamp(maxEntries, 100, 10_000);
    }

    /// <summary>Appends a log line. Call from your logging sink (e.g. Serilog).</summary>
    public void Append(string entry)
    {
        if (string.IsNullOrEmpty(entry)) return;
        _entries.Enqueue(entry);
        if (Interlocked.Increment(ref _count) > _maxEntries)
        {
            _entries.TryDequeue(out _);
            Interlocked.Decrement(ref _count);
        }
    }

    public IReadOnlyList<string> GetRecentEntries(int count)
    {
        var capped = Math.Clamp(count, 0, SupportBundleService.MaxLogEntriesCap);
        var list = _entries.TakeLast(capped).ToList();
        return list;
    }
}
