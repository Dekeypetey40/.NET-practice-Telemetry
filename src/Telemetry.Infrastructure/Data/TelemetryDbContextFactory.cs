using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Telemetry.Infrastructure.Data;

public class TelemetryDbContextFactory : IDesignTimeDbContextFactory<TelemetryDbContext>
{
    public TelemetryDbContext CreateDbContext(string[] args)
    {
        var cur = Directory.GetCurrentDirectory();
        var basePath = Directory.Exists(Path.Combine(cur, "src", "Telemetry.Api"))
            ? Path.Combine(cur, "src", "Telemetry.Api")
            : Path.Combine(cur, "..", "src", "Telemetry.Api");
        if (!Directory.Exists(basePath))
            basePath = cur;
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=telemetry;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<TelemetryDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TelemetryDbContext(options);
    }
}
