using UptimeMonitor.Application.DTOs;
using UptimeMonitor.Application.UseCases;

namespace UptimeMonitor.Application.Tests;

public sealed class RegisterEndpointTests
{
    [Fact]
    public async Task ExecuteAsync_ValidRequest_ReturnsSummaryAndPersistsEndpoint()
    {
        var unitOfWork = new InMemoryUnitOfWork();
        var repository = new InMemoryEndpointRepository(unitOfWork);
        var useCase = new RegisterEndpoint(repository, unitOfWork);
        var request = new RegisterEndpointRequest("Public API", "https://example.com/health", TimeSpan.FromMinutes(2));

        var summary = await useCase.ExecuteAsync(request);

        var stagedEndpoint = Assert.Single(repository.StagedAddedEndpoints);
        var persistedEndpoint = Assert.Single(repository.PersistedAddedEndpoints);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(summary.Id, persistedEndpoint.Id);
        Assert.Equal("Public API", summary.Name);
        Assert.Equal("https://example.com/health", summary.Url);
        Assert.Equal(TimeSpan.FromMinutes(2), summary.CheckInterval);
        Assert.Null(summary.LastCheckedAt);
        Assert.True(summary.IsEnabled);
        Assert.Equal(summary.Id, stagedEndpoint.Id);
        Assert.Equal(summary.Name, persistedEndpoint.Name);
        Assert.Equal(summary.Url, persistedEndpoint.Url);
        Assert.Equal(summary.CheckInterval, persistedEndpoint.CheckInterval);
    }
}