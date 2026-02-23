using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Telemetry.Application.DTOs;
using Xunit;

namespace Telemetry.IntegrationTests;

public class RunsControllerTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;

    public RunsControllerTests(IntegrationTestFixture fixture) => _client = fixture.Client;

    [Fact]
    public async Task CreateRun_Queue_Start_Get_Returns_Timeline()
    {
        var instrumentResponse = await _client.PostAsJsonAsync("/instruments", new CreateInstrumentRequest("LC-1", "Chromatograph", "SN-001"));
        instrumentResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var instrument = await instrumentResponse.Content.ReadFromJsonAsync<InstrumentHealthResponse>();
        instrument.Should().NotBeNull();
        var instrumentId = instrument!.InstrumentId;

        var createResponse = await _client.PostAsJsonAsync("/runs", new CreateRunRequest(instrumentId, "S-001", "MethodA", "1.0"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var run = await createResponse.Content.ReadFromJsonAsync<RunResponse>();
        run.Should().NotBeNull();
        run!.CurrentState.Should().Be("Created");
        var runId = run.Id;

        var queueResponse = await _client.PostAsync($"/runs/{runId}/queue", null);
        queueResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var queued = await queueResponse.Content.ReadFromJsonAsync<RunResponse>();
        queued!.CurrentState.Should().Be("Queued");

        var startResponse = await _client.PostAsync($"/runs/{runId}/start", null);
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var started = await startResponse.Content.ReadFromJsonAsync<RunResponse>();
        started!.CurrentState.Should().Be("Running");

        var getResponse = await _client.GetAsync($"/runs/{runId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var got = await getResponse.Content.ReadFromJsonAsync<RunResponse>();
        got!.CurrentState.Should().Be("Running");

        var timelineResponse = await _client.GetAsync($"/runs/{runId}/timeline");
        timelineResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var timeline = await timelineResponse.Content.ReadFromJsonAsync<RunTimelineResponse>();
        timeline!.RunId.Should().Be(runId);
        timeline.Events.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Start_WhenNotQueued_Returns409()
    {
        var instrumentResponse = await _client.PostAsJsonAsync("/instruments", new CreateInstrumentRequest("I2", "Type", null));
        instrumentResponse.EnsureSuccessStatusCode();
        var instrument = await instrumentResponse.Content.ReadFromJsonAsync<InstrumentHealthResponse>();
        var createResponse = await _client.PostAsJsonAsync("/runs", new CreateRunRequest(instrument!.InstrumentId, "S-2"));
        createResponse.EnsureSuccessStatusCode();
        var run = await createResponse.Content.ReadFromJsonAsync<RunResponse>();
        var runId = run!.Id;

        var startResponse = await _client.PostAsync($"/runs/{runId}/start", null);
        startResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
