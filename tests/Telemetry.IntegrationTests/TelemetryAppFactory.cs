using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Telemetry.IntegrationTests;

public class TelemetryAppFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public TelemetryAppFactory(string connectionString) => _connectionString = connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        // Ensure connection string is available in CI (config and env so host startup never fails).
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
