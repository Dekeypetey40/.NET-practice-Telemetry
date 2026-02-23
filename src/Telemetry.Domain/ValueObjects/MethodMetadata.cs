namespace Telemetry.Domain.ValueObjects;

public sealed record MethodMetadata(string MethodName, string? Version = null, IReadOnlyDictionary<string, string>? Parameters = null);
