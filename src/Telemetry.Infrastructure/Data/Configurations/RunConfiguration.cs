using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Telemetry.Domain.Entities;

namespace Telemetry.Infrastructure.Data.Configurations;

public class RunConfiguration : IEntityTypeConfiguration<Run>
{
    public void Configure(EntityTypeBuilder<Run> builder)
    {
        builder.ToTable("Runs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SampleId).HasMaxLength(256).IsRequired();
        builder.Property(e => e.MethodMetadataJson).HasColumnType("jsonb").IsRequired();
        builder.Property(e => e.CurrentState).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(e => e.Actor).HasMaxLength(256);
        builder.Property(e => e.CorrelationId).HasMaxLength(128);
        builder.HasMany(e => e.Events).WithOne().HasForeignKey(ev => ev.RunId).OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(Run.Events))!.SetField("_events");
    }
}
