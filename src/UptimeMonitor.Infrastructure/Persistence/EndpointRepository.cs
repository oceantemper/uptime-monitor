using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Application.Abstractions.Persistence;
using UptimeMonitor.Domain;

namespace UptimeMonitor.Infrastructure.Persistence;

public sealed class EndpointRepository(UptimeMonitorDbContext dbContext) : IEndpointRepository
{
    public async Task AddAsync(MonitoredEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        await dbContext.Endpoints.AddAsync(endpoint, cancellationToken);
    }

    public Task<MonitoredEndpoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Endpoints.SingleOrDefaultAsync(endpoint => endpoint.Id == id, cancellationToken);

    public async Task<IReadOnlyList<MonitoredEndpoint>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Endpoints
            .OrderBy(endpoint => endpoint.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<MonitoredEndpoint>> GetDueForCheckingAsync(
        DateTimeOffset at,
        CancellationToken cancellationToken = default)
    {
        // This loads every enabled endpoint; replace with a database-side due query if endpoint volume makes that scale poorly.
        var enabledEndpoints = await dbContext.Endpoints
            .Where(endpoint => endpoint.IsEnabled)
            .ToListAsync(cancellationToken);

        return enabledEndpoints.Where(endpoint => endpoint.IsDue(at)).ToList();
    }

    public Task UpdateAsync(MonitoredEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        dbContext.Entry(endpoint).State = EntityState.Modified;
        return Task.CompletedTask;
    }
}
