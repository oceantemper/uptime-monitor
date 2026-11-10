using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Domain;
using UptimeMonitor.Infrastructure.Persistence;

namespace UptimeMonitor.Infrastructure.Tests;

internal sealed class PostgresTestData(DbContextOptions<UptimeMonitorDbContext> options) : IAsyncDisposable
{
    private readonly HashSet<Guid> endpointIds = [];

    public UptimeMonitorDbContext CreateDbContext() => new(options);

    public MonitoredEndpoint CreateEndpoint(string name, string url, TimeSpan interval)
    {
        var endpoint = new MonitoredEndpoint(name, url, interval);
        endpointIds.Add(endpoint.Id);
        return endpoint;
    }

    public async ValueTask DisposeAsync()
    {
        if (endpointIds.Count == 0)
        {
            return;
        }

        try
        {
            await using var dbContext = CreateDbContext();
            var ids = endpointIds.ToArray();
            await dbContext.CheckResults
                .Where(result => ids.Contains(result.EndpointId))
                .ExecuteDeleteAsync();
            await dbContext.Endpoints
                .Where(endpoint => ids.Contains(endpoint.Id))
                .ExecuteDeleteAsync();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Could not clean up records inserted by this PostgreSQL integration test ({exception.GetType().Name}).");
        }
    }
}