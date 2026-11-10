using UptimeMonitor.Domain;

namespace UptimeMonitor.Application.Abstractions.Persistence;

public interface IEndpointRepository
{
    /// <summary>Stages an endpoint addition for the current unit of work.</summary>
    Task AddAsync(MonitoredEndpoint endpoint, CancellationToken cancellationToken = default);

    Task<MonitoredEndpoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonitoredEndpoint>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonitoredEndpoint>> GetDueForCheckingAsync(
        DateTimeOffset at,
        CancellationToken cancellationToken = default);

    /// <summary>Stages endpoint changes for the current unit of work.</summary>
    Task UpdateAsync(MonitoredEndpoint endpoint, CancellationToken cancellationToken = default);
}