using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Telemetry.Infrastructure.Data;

namespace Telemetry.IntegrationTests;

public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("telemetry")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var connectionString = _container.GetConnectionString();
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connectionString);
        await ApplyMigrationsAsync(connectionString);
        var factory = new TelemetryAppFactory(connectionString);
        Client = factory.CreateClient();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    private static async Task ApplyMigrationsAsync(string connectionString)
    {
        var options = new DbContextOptionsBuilder<TelemetryDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        await using var db = new TelemetryDbContext(options);
        await db.Database.MigrateAsync();
    }
}
