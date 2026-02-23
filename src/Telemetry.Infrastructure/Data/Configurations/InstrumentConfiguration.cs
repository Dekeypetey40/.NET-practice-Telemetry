using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Telemetry.Domain.Entities;

namespace Telemetry.Infrastructure.Data.Configurations;

public class InstrumentConfiguration : IEntityTypeConfiguration<Instrument>
{
    public void Configure(EntityTypeBuilder<Instrument> builder)
    {
        builder.ToTable("Instruments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Type).HasMaxLength(128).IsRequired();
        builder.Property(e => e.SerialNumber).HasMaxLength(128);
        builder.Property(e => e.Status).HasMaxLength(64).IsRequired();
        builder.HasMany(e => e.Alarms).WithOne().HasForeignKey(a => a.InstrumentId).IsRequired(false);
    }
}
