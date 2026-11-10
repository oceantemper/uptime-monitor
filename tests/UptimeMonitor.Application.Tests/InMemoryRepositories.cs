using UptimeMonitor.Application.Abstractions.Persistence;
using UptimeMonitor.Domain;

namespace UptimeMonitor.Application.Tests;

internal sealed class InMemoryEndpointRepository : IEndpointRepository
{
    private readonly Dictionary<Guid, MonitoredEndpoint> _endpoints = [];

    public List<MonitoredEndpoint> AddedEndpoints { get; } = [];

    public List<MonitoredEndpoint> UpdatedEndpoints { get; } = [];

    public void Seed(MonitoredEndpoint endpoint) => _endpoints.Add(endpoint.Id, endpoint);

    public Task AddAsync(MonitoredEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        AddedEndpoints.Add(endpoint);
        _endpoints.Add(endpoint.Id, endpoint);
        return Task.CompletedTask;
    }

    public Task<MonitoredEndpoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _endpoints.TryGetValue(id, out var endpoint);
        return Task.FromResult(endpoint);
    }

    public Task<IReadOnlyList<MonitoredEndpoint>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<MonitoredEndpoint>>(_endpoints.Values.ToList());

    public Task<IReadOnlyList<MonitoredEndpoint>> GetDueForCheckingAsync(
        DateTimeOffset at,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<MonitoredEndpoint>>(_endpoints.Values.Where(endpoint => endpoint.IsDue(at)).ToList());

    public Task UpdateAsync(MonitoredEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        UpdatedEndpoints.Add(endpoint);
        _endpoints[endpoint.Id] = endpoint;
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryCheckResultRepository : ICheckResultRepository
{
    public List<CheckResult> AddedResults { get; } = [];

    public Task AddAsync(CheckResult result, CancellationToken cancellationToken = default)
    {
        AddedResults.Add(result);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CheckResult>> GetForEndpointSinceAsync(
        Guid endpointId,
        DateTimeOffset since,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<CheckResult>>(
            AddedResults.Where(result => result.EndpointId == endpointId && result.CheckedAt >= since).ToList());
}