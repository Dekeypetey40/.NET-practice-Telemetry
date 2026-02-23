namespace Telemetry.Domain.Entities;

public class Instrument
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string? SerialNumber { get; private set; }
    public string Status { get; private set; } = "Unknown";
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastHealthCheck { get; private set; }

    private readonly List<Alarm> _alarms = new();
    public IReadOnlyCollection<Alarm> Alarms => _alarms.AsReadOnly();

    private Instrument() { }

    public static Instrument Create(string name, string type, string? serialNumber = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Instrument name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Instrument type cannot be empty.", nameof(type));

        return new Instrument
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Type = type.Trim(),
            SerialNumber = serialNumber?.Trim(),
            Status = "Unknown",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateHealth(string status, DateTime? lastHealthCheck = null)
    {
        Status = status ?? "Unknown";
        LastHealthCheck = lastHealthCheck ?? DateTime.UtcNow;
    }
}
