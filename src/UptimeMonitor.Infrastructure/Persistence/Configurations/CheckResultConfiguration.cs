using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UptimeMonitor.Domain;

namespace UptimeMonitor.Infrastructure.Persistence.Configurations;

internal sealed class CheckResultConfiguration : IEntityTypeConfiguration<CheckResult>
{
    public void Configure(EntityTypeBuilder<CheckResult> builder)
    {
        builder.ToTable("check_results");
        builder.HasKey(result => result.Id);
        builder.Property(result => result.Id).ValueGeneratedNever();
        builder.Property(result => result.EndpointId).IsRequired();
        builder.Property(result => result.CheckedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(result => result.Duration)
            .HasColumnType("interval")
            .IsRequired();
        builder.Property(result => result.Outcome)
            .HasConversion<string>()
            .HasColumnType("text")
            .IsRequired();
        builder.Property(result => result.HttpStatusCode);
        builder.Property(result => result.FailureDetail).HasColumnType("text");

        builder.HasOne<MonitoredEndpoint>()
            .WithMany()
            .HasForeignKey(result => result.EndpointId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(result => new { result.EndpointId, result.CheckedAt });
    }
}
