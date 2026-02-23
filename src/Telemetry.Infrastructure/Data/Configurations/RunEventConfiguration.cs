using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Telemetry.Domain.Entities;

namespace Telemetry.Infrastructure.Data.Configurations;

public class RunEventConfiguration : IEntityTypeConfiguration<RunEvent>
{
    public void Configure(EntityTypeBuilder<RunEvent> builder)
    {
        builder.ToTable("RunEvents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EventType).HasMaxLength(64).IsRequired();
        builder.Property(e => e.Data).HasMaxLength(2048);
        builder.Property(e => e.Actor).HasMaxLength(256);
        builder.Property(e => e.CorrelationId).HasMaxLength(128);
    }
}
