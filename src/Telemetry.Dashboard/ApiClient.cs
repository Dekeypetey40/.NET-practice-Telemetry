using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Telemetry.Dashboard;

/// <summary>
/// Thin client for the Telemetry API. Used by the dashboard to list runs, get details, and trigger transitions.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ApiClient(string baseAddress)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseAddress.TrimEnd('/') + "/") };
        _http.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<IReadOnlyList<RunDto>> GetRunsAsync(int limit = 50, CancellationToken ct = default)
    {
        var list = await _http.GetFromJsonAsync<List<RunDto>>($"runs?limit={limit}", JsonOptions, ct);
        return list ?? new List<RunDto>();
    }

    public async Task<RunDto?> GetRunAsync(Guid id, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<RunDto>($"runs/{id}", JsonOptions, ct);
    }

    public async Task<RunTimelineDto?> GetTimelineAsync(Guid id, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<RunTimelineDto>($"runs/{id}/timeline", JsonOptions, ct);
    }

    public async Task<RunDto?> QueueAsync(Guid id, string? actor = null, CancellationToken ct = default)
    {
        var url = actor != null ? $"runs/{id}/queue?actor={Uri.EscapeDataString(actor)}" : $"runs/{id}/queue";
        var response = await _http.PostAsync(url, null, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RunDto>(JsonOptions, ct);
    }

    public async Task<RunDto?> StartAsync(Guid id, string? actor = null, CancellationToken ct = default)
    {
        var url = actor != null ? $"runs/{id}/start?actor={Uri.EscapeDataString(actor)}" : $"runs/{id}/start";
        var response = await _http.PostAsync(url, null, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RunDto>(JsonOptions, ct);
    }

    public async Task<RunDto?> CancelAsync(Guid id, string? actor = null, CancellationToken ct = default)
    {
        var url = actor != null ? $"runs/{id}/cancel?actor={Uri.EscapeDataString(actor)}" : $"runs/{id}/cancel";
        var response = await _http.PostAsync(url, null, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RunDto>(JsonOptions, ct);
    }
}

public record RunDto(Guid Id, Guid InstrumentId, string SampleId, string MethodMetadataJson, string CurrentState,
    DateTime CreatedAt, DateTime? StartedAt, DateTime? CompletedAt, string? Actor, string? CorrelationId);

public record RunTimelineEventDto(Guid Id, string EventType, DateTime Timestamp, string? Data, string? Actor, string? CorrelationId);

public record RunTimelineDto(Guid RunId, IReadOnlyList<RunTimelineEventDto> Events);
