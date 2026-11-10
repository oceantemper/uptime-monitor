using UptimeMonitor.Application.Abstractions.Persistence;
using UptimeMonitor.Domain;

namespace UptimeMonitor.Application.Tests;

internal sealed class InMemoryUnitOfWork : IUnitOfWork
{
    private readonly List<Action> _pendingChanges = [];

    public int SaveChangesCallCount { get; private set; }

    public void Stage(Action commit) => _pendingChanges.Add(commit);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;

        foreach (var commit in _pendingChanges)
        {
            commit();
        }

        _pendingChanges.Clear();
        return Task.CompletedTask;
    }
}

internal sealed record EndpointSnapshot(
    Guid Id,
    string Name,
    string Url,
    TimeSpan CheckInterval,
    DateTimeOffset? LastCheckedAt,
    bool IsEnabled);

internal sealed class InMemoryEndpointRepository : IEndpointRepository
{
    private readonly InMemoryUnitOfWork _unitOfWork;
    private readonly Dictionary<Guid, MonitoredEndpoint> _endpoints = [];
    private readonly Dictionary<Guid, EndpointSnapshot> _persistedEndpoints = [];

    public InMemoryEndpointRepository(InMemoryUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public List<MonitoredEndpoint> StagedAddedEndpoints { get; } = [];

    public List<MonitoredEndpoint> StagedUpdatedEndpoints { get; } = [];

    public List<EndpointSnapshot> PersistedAddedEndpoints { get; } = [];

    public IReadOnlyCollection<EndpointSnapshot> PersistedEndpoints => _persistedEndpoints.Values.ToList();

    public void Seed(MonitoredEndpoint endpoint)
    {
        _endpoints.Add(endpoint.Id, endpoint);
        _persistedEndpoints.Add(endpoint.Id, Snapshot(endpoint));
    }

    public Task AddAsync(MonitoredEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        StagedAddedEndpoints.Add(endpoint);
        var persistedEndpoint = Snapshot(endpoint);
        _unitOfWork.Stage(() =>
        {
            PersistedAddedEndpoints.Add(persistedEndpoint);
            _endpoints.Add(persistedEndpoint.Id, endpoint);
            _persistedEndpoints.Add(persistedEndpoint.Id, persistedEndpoint);
        });
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
        Task.FromResult<IReadOnlyList<MonitoredEndpoint>>(
            _endpoints.Values.Where(endpoint => endpoint.IsDue(at)).ToList());

    public Task UpdateAsync(MonitoredEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        StagedUpdatedEndpoints.Add(endpoint);
        var persistedEndpoint = Snapshot(endpoint);
        _unitOfWork.Stage(() =>
        {
            _endpoints[persistedEndpoint.Id] = endpoint;
            _persistedEndpoints[persistedEndpoint.Id] = persistedEndpoint;
        });
        return Task.CompletedTask;
    }

    private static EndpointSnapshot Snapshot(MonitoredEndpoint endpoint) => new(
        endpoint.Id,
        endpoint.Name,
        endpoint.Url.ToString(),
        endpoint.CheckInterval,
        endpoint.LastCheckedAt,
        endpoint.IsEnabled);
}

internal sealed class InMemoryCheckResultRepository : ICheckResultRepository
{
    private readonly InMemoryUnitOfWork _unitOfWork;

    public InMemoryCheckResultRepository(InMemoryUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public List<CheckResult> StagedAddedResults { get; } = [];

    public List<CheckResult> PersistedResults { get; } = [];

    public Task AddAsync(CheckResult result, CancellationToken cancellationToken = default)
    {
        StagedAddedResults.Add(result);
        _unitOfWork.Stage(() => PersistedResults.Add(result));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CheckResult>> GetForEndpointSinceAsync(
        Guid endpointId,
        DateTimeOffset since,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<CheckResult>>(
            PersistedResults.Where(result => result.EndpointId == endpointId && result.CheckedAt >= since).ToList());
}