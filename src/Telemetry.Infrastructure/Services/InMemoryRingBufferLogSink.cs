using Serilog.Core;
using Serilog.Events;

namespace Telemetry.Infrastructure.Services;

/// <summary>
/// Serilog sink that appends formatted log events to an in-memory ring buffer for support bundles.
/// </summary>
public sealed class InMemoryRingBufferLogSink : ILogEventSink
{
    private readonly InMemoryRingBufferLogCollector _collector;

    public InMemoryRingBufferLogSink(InMemoryRingBufferLogCollector collector)
    {
        _collector = collector;
    }

    public void Emit(LogEvent logEvent)
    {
        var line = logEvent.Timestamp.ToString("O") + " [" + logEvent.Level + "] "
            + (logEvent.MessageTemplate.Render(logEvent.Properties))
            + (logEvent.Exception != null ? Environment.NewLine + logEvent.Exception : "");
        _collector.Append(line);
    }
}
