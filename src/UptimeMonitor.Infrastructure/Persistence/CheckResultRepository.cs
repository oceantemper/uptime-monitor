using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Application.Abstractions.Persistence;
using UptimeMonitor.Domain;

namespace UptimeMonitor.Infrastructure.Persistence;

public sealed class CheckResultRepository(UptimeMonitorDbContext dbContext) : ICheckResultRepository
{
    public async Task AddAsync(CheckResult result, CancellationToken cancellationToken = default)
    {
        await dbContext.CheckResults.AddAsync(result, cancellationToken);
    }

    public async Task<IReadOnlyList<CheckResult>> GetForEndpointSinceAsync(
        Guid endpointId,
        DateTimeOffset since,
        CancellationToken cancellationToken = default) =>
        await dbContext.CheckResults
            .Where(result => result.EndpointId == endpointId && result.CheckedAt >= since)
            .OrderBy(result => result.CheckedAt)
            .ToListAsync(cancellationToken);
}
