using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;

namespace Telemetry.Infrastructure.Data;

public class TelemetryDbContext : DbContext
{
    public TelemetryDbContext(DbContextOptions<TelemetryDbContext> options) : base(options) { }

    public DbSet<Instrument> Instruments => Set<Instrument>();
    public DbSet<Run> Runs => Set<Run>();
    public DbSet<RunEvent> RunEvents => Set<RunEvent>();
    public DbSet<Alarm> Alarms => Set<Alarm>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TelemetryDbContext).Assembly);
    }
}
