using UptimeMonitor.Application.DTOs;
using UptimeMonitor.Application.UseCases;

namespace UptimeMonitor.Application.Tests;

public sealed class RegisterEndpointTests
{
    [Fact]
    public async Task ExecuteAsync_ValidRequest_ReturnsSummaryAndPersistsEndpoint()
    {
        var repository = new InMemoryEndpointRepository();
        var useCase = new RegisterEndpoint(repository);
        var request = new RegisterEndpointRequest("Public API", "https://example.com/health", TimeSpan.FromMinutes(2));

        var summary = await useCase.ExecuteAsync(request);

        var persistedEndpoint = Assert.Single(repository.AddedEndpoints);
        Assert.Equal(persistedEndpoint.Id, summary.Id);
        Assert.Equal("Public API", summary.Name);
        Assert.Equal("https://example.com/health", summary.Url);
        Assert.Equal(TimeSpan.FromMinutes(2), summary.CheckInterval);
        Assert.Null(summary.LastCheckedAt);
        Assert.True(summary.IsEnabled);
        Assert.Equal(summary.Id, persistedEndpoint.Id);
        Assert.Equal(summary.Name, persistedEndpoint.Name);
        Assert.Equal(summary.Url, persistedEndpoint.Url.ToString());
        Assert.Equal(summary.CheckInterval, persistedEndpoint.CheckInterval);
    }
}