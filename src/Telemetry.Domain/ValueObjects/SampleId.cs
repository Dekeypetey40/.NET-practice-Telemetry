namespace Telemetry.Domain.ValueObjects;

public sealed record SampleId(string Value)
{
    public static SampleId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Sample ID cannot be empty.", nameof(value));
        return new SampleId(value.Trim());
    }
}
