using UptimeMonitor.Domain;

namespace UptimeMonitor.Application.Abstractions.Persistence;

public interface ICheckResultRepository
{
    /// <summary>Stages a check result addition for the current unit of work.</summary>
    Task AddAsync(CheckResult result, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CheckResult>> GetForEndpointSinceAsync(
        Guid endpointId,
        DateTimeOffset since,
        CancellationToken cancellationToken = default);
}