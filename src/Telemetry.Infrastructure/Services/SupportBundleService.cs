using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Telemetry.Application.Contracts;
using Telemetry.Domain.Entities;

namespace Telemetry.Infrastructure.Services;

public class SupportBundleService : ISupportBundleService
{
    private readonly IRunRepository _runRepository;
    private readonly ISupportBundleLogCollector? _logCollector;
    private readonly IHostEnvironment? _hostEnvironment;

    public SupportBundleService(
        IRunRepository runRepository,
        ISupportBundleLogCollector? logCollector = null,
        IHostEnvironment? hostEnvironment = null)
    {
        _runRepository = runRepository;
        _logCollector = logCollector;
        _hostEnvironment = hostEnvironment;
    }

    /// <summary>Maximum log entries to include in a bundle; prevents excessive memory use.</summary>
    public const int MaxLogEntriesCap = 1000;

    public async Task<Stream> CreateBundleForRunAsync(Guid runId, int lastLogEntriesCount = 100, CancellationToken cancellationToken = default)
    {
        var logCount = Math.Clamp(lastLogEntriesCount, 1, MaxLogEntriesCap);

        var run = await _runRepository.GetByIdAsync(runId, includeEvents: true, cancellationToken);
        if (run == null)
            throw new KeyNotFoundException($"Run {runId} not found.");

        var timeline = run.Events.OrderBy(e => e.Timestamp).ToList();

        var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddMetadataEntry(zip, run);
            AddTimelineEntry(zip, timeline);
            AddEnvironmentEntry(zip);
            if (_logCollector != null)
                AddLogsEntry(zip, logCount);
        }
        stream.Position = 0;
        return stream;
    }

    private static void AddMetadataEntry(ZipArchive zip, Run run)
    {
        var metadata = new
        {
            run.Id,
            run.InstrumentId,
            run.SampleId,
            run.MethodMetadataJson,
            CurrentState = run.CurrentState.ToString(),
            run.CreatedAt,
            run.StartedAt,
            run.CompletedAt,
            run.Actor,
            run.CorrelationId
        };
        var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
        AddTextEntry(zip, "metadata.json", json);
    }

    private static void AddTimelineEntry(ZipArchive zip, IReadOnlyList<RunEvent> timeline)
    {
        var items = timeline.Select(e => new
        {
            e.Id,
            e.RunId,
            e.EventType,
            e.Timestamp,
            e.Data,
            e.Actor,
            e.CorrelationId
        }).ToList();
        var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
        AddTextEntry(zip, "timeline.json", json);
    }

    private void AddEnvironmentEntry(ZipArchive zip)
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0";
        var env = new
        {
            ApplicationVersion = version,
            Environment = _hostEnvironment?.EnvironmentName ?? "Unknown",
            MachineName = Environment.MachineName,
            CollectedAt = DateTime.UtcNow
        };
        var json = JsonSerializer.Serialize(env, new JsonSerializerOptions { WriteIndented = true });
        AddTextEntry(zip, "environment.json", json);
    }

    private void AddLogsEntry(ZipArchive zip, int count)
    {
        var entries = _logCollector!.GetRecentEntries(count);
        var text = string.Join(Environment.NewLine, entries);
        AddTextEntry(zip, "logs.txt", text);
    }

    private static void AddTextEntry(ZipArchive zip, string name, string content)
    {
        var entry = zip.CreateEntry(name);
        using var w = new StreamWriter(entry.Open(), Encoding.UTF8);
        w.Write(content);
    }
}
