namespace Telemetry.Domain.Entities;

public class RunEvent
{
    public Guid Id { get; private set; }
    public Guid RunId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }
    public string? Data { get; private set; }
    public string? Actor { get; private set; }
    public string? CorrelationId { get; private set; }

    private RunEvent() { }

    public static RunEvent Create(Guid runId, string eventType, string? data = null, string? actor = null, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type cannot be empty.", nameof(eventType));

        return new RunEvent
        {
            Id = Guid.NewGuid(),
            RunId = runId,
            EventType = eventType.Trim(),
            Timestamp = DateTime.UtcNow,
            Data = data,
            Actor = actor,
            CorrelationId = correlationId
        };
    }
}
