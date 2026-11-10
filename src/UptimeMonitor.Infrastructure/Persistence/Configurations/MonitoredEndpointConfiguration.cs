using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UptimeMonitor.Domain;

namespace UptimeMonitor.Infrastructure.Persistence.Configurations;

internal sealed class MonitoredEndpointConfiguration : IEntityTypeConfiguration<MonitoredEndpoint>
{
    public void Configure(EntityTypeBuilder<MonitoredEndpoint> builder)
    {
        builder.ToTable("monitored_endpoints");
        builder.HasKey(endpoint => endpoint.Id);
        builder.Property(endpoint => endpoint.Id).ValueGeneratedNever();
        builder.Property(endpoint => endpoint.Name).IsRequired();
        builder.Property(endpoint => endpoint.Url)
            .HasConversion(url => url.AbsoluteUri, value => new Uri(value, UriKind.Absolute))
            .HasColumnType("text")
            .IsRequired();
        builder.Property(endpoint => endpoint.CheckInterval)
            .HasColumnType("interval")
            .IsRequired();
        builder.Property(endpoint => endpoint.LastCheckedAt).HasColumnType("timestamp with time zone");
        builder.Property(endpoint => endpoint.IsEnabled).IsRequired();
        builder.HasIndex(endpoint => endpoint.IsEnabled);
    }
}
