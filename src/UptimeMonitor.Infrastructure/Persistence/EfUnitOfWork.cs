using UptimeMonitor.Application.Abstractions.Persistence;

namespace UptimeMonitor.Infrastructure.Persistence;

public sealed class EfUnitOfWork(UptimeMonitorDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
