using UptimeMonitor.Domain;

namespace UptimeMonitor.Application.Abstractions.Persistence;

public interface IEndpointRepository
{
    Task AddAsync(MonitoredEndpoint endpoint, CancellationToken cancellationToken = default);

    Task<MonitoredEndpoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonitoredEndpoint>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonitoredEndpoint>> GetDueForCheckingAsync(
        DateTimeOffset at,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(MonitoredEndpoint endpoint, CancellationToken cancellationToken = default);
}