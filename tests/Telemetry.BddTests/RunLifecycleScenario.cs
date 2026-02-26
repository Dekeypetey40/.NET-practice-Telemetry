using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Telemetry.Infrastructure.Data;

namespace Telemetry.BddTests;

/// <summary>
/// BDD-style scenario: Given an instrument and a created run, when we queue, start, and complete,
/// then the run is in Completed state and the timeline reflects all transitions.
/// </summary>
public class RunLifecycleScenario
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static async Task ApplyMigrationsAsync(string connectionString)
    {
        var options = new DbContextOptionsBuilder<TelemetryDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        await using var db = new TelemetryDbContext(options);
        await db.Database.MigrateAsync();
    }

    [Fact]
    public async Task Run_lifecycle_create_queue_start_complete_succeeds()
    {
        await using var container = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("telemetry")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        await container.StartAsync();
        var connectionString = container.GetConnectionString();
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connectionString);
        await ApplyMigrationsAsync(connectionString);
        using var factory = new BddAppFactory(connectionString);
        var client = factory.CreateClient();

        // Given: an instrument and a created run
        var instrumentResponse = await client.PostAsJsonAsync("instruments", new
        {
            Name = "BDD-Instrument",
            Type = "Test",
            SerialNumber = "BDD-001"
        }, JsonOptions);
        instrumentResponse.EnsureSuccessStatusCode();
        var instrument = await instrumentResponse.Content.ReadFromJsonAsync<InstrumentDto>(JsonOptions);
        Assert.NotNull(instrument);

        var createRunResponse = await client.PostAsJsonAsync("runs", new
        {
            InstrumentId = instrument.InstrumentId,
            SampleId = "BDD-Sample-001"
        }, JsonOptions);
        createRunResponse.EnsureSuccessStatusCode();
        var run = await createRunResponse.Content.ReadFromJsonAsync<RunDto>(JsonOptions);
        Assert.NotNull(run);
        Assert.Equal("Created", run.CurrentState);

        // When: queue, then start, then complete
        var queueResponse = await client.PostAsync($"runs/{run.Id}/queue", null);
        queueResponse.EnsureSuccessStatusCode();
        run = await queueResponse.Content.ReadFromJsonAsync<RunDto>(JsonOptions);
        Assert.NotNull(run);
        Assert.Equal("Queued", run.CurrentState);

        var startResponse = await client.PostAsync($"runs/{run.Id}/start", null);
        startResponse.EnsureSuccessStatusCode();
        run = await startResponse.Content.ReadFromJsonAsync<RunDto>(JsonOptions);
        Assert.NotNull(run);
        Assert.Equal("Running", run.CurrentState);

        var completeResponse = await client.PostAsync($"runs/{run.Id}/complete", null);
        completeResponse.EnsureSuccessStatusCode();
        run = await completeResponse.Content.ReadFromJsonAsync<RunDto>(JsonOptions);
        Assert.NotNull(run);

        // Then: run is Completed and timeline has the expected events
        Assert.Equal("Completed", run.CurrentState);
        Assert.NotNull(run.CompletedAt);

        var timelineResponse = await client.GetAsync($"runs/{run.Id}/timeline");
        timelineResponse.EnsureSuccessStatusCode();
        var timeline = await timelineResponse.Content.ReadFromJsonAsync<TimelineDto>(JsonOptions);
        Assert.NotNull(timeline);
        var dataEntries = timeline.Events.Select(e => e.Data ?? "").ToList();
        Assert.Contains("Created→Queued", dataEntries);
        Assert.Contains("Queued→Running", dataEntries);
        Assert.Contains("Running→Completed", dataEntries);
    }

    private sealed class BddAppFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;

        public BddAppFactory(string connectionString) => _connectionString = connectionString;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _connectionString);
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _connectionString
                });
            });
        }
    }

    private record InstrumentDto(Guid InstrumentId, string Name, string Type, string? SerialNumber, string Status);
    private record RunDto(Guid Id, Guid InstrumentId, string SampleId, string CurrentState, DateTime CreatedAt, DateTime? StartedAt, DateTime? CompletedAt);
    private record TimelineEventDto(Guid Id, string EventType, DateTime Timestamp, string? Data);
    private record TimelineDto(Guid RunId, IReadOnlyList<TimelineEventDto> Events);
}
