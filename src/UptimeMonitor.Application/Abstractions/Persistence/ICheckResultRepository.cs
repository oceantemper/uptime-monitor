using UptimeMonitor.Domain;

namespace UptimeMonitor.Application.Abstractions.Persistence;

public interface ICheckResultRepository
{
    Task AddAsync(CheckResult result, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CheckResult>> GetForEndpointSinceAsync(
        Guid endpointId,
        DateTimeOffset since,
        CancellationToken cancellationToken = default);
}