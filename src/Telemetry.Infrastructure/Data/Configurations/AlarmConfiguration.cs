using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Telemetry.Domain.Entities;

namespace Telemetry.Infrastructure.Data.Configurations;

public class AlarmConfiguration : IEntityTypeConfiguration<Alarm>
{
    public void Configure(EntityTypeBuilder<Alarm> builder)
    {
        builder.ToTable("Alarms");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Severity).HasMaxLength(32).IsRequired();
        builder.Property(e => e.Message).HasMaxLength(1024).IsRequired();
        builder.Property(e => e.CorrelationId).HasMaxLength(128);
    }
}
