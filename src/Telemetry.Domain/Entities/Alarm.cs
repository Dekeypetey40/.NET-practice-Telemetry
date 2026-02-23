namespace Telemetry.Domain.Entities;

public class Alarm
{
    public Guid Id { get; private set; }
    public Guid InstrumentId { get; private set; }
    public string Severity { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public DateTime RaisedAt { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public string? CorrelationId { get; private set; }

    private Alarm() { }

    public static Alarm Raise(Guid instrumentId, string severity, string message, string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(severity))
            throw new ArgumentException("Severity cannot be empty.", nameof(severity));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty.", nameof(message));

        return new Alarm
        {
            Id = Guid.NewGuid(),
            InstrumentId = instrumentId,
            Severity = severity.Trim(),
            Message = message.Trim(),
            RaisedAt = DateTime.UtcNow,
            CorrelationId = correlationId
        };
    }

    public void Acknowledge()
    {
        AcknowledgedAt = DateTime.UtcNow;
    }
}
