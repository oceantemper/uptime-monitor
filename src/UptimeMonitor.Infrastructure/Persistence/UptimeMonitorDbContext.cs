using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Domain;

namespace UptimeMonitor.Infrastructure.Persistence;

public sealed class UptimeMonitorDbContext(DbContextOptions<UptimeMonitorDbContext> options) : DbContext(options)
{
    public DbSet<MonitoredEndpoint> Endpoints => Set<MonitoredEndpoint>();

    public DbSet<CheckResult> CheckResults => Set<CheckResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UptimeMonitorDbContext).Assembly);
    }
}
