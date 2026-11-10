using UptimeMonitor.Application.DTOs;
using UptimeMonitor.Application.Exceptions;
using UptimeMonitor.Application.UseCases;
using UptimeMonitor.Domain;
using ApplicationCheckOutcome = UptimeMonitor.Application.DTOs.CheckOutcome;
using DomainCheckOutcome = UptimeMonitor.Domain.CheckOutcome;

namespace UptimeMonitor.Application.Tests;

public sealed class RecordCheckResultTests
{
    [Theory]
    [InlineData(ApplicationCheckOutcome.Succeeded, DomainCheckOutcome.Succeeded)]
    [InlineData(ApplicationCheckOutcome.Failed, DomainCheckOutcome.Failed)]
    public async Task ExecuteAsync_ExistingEndpoint_MapsOutcomePersistsResultAndReturnsSummary(
        ApplicationCheckOutcome applicationOutcome,
        DomainCheckOutcome expectedDomainOutcome)
    {
        var endpointRepository = new InMemoryEndpointRepository();
        var checkResultRepository = new InMemoryCheckResultRepository();
        var endpoint = new MonitoredEndpoint("Public API", "https://example.com/health", TimeSpan.FromMinutes(1));
        endpointRepository.Seed(endpoint);
        var useCase = new RecordCheckResult(endpointRepository, checkResultRepository);
        var checkedAt = new DateTimeOffset(2026, 11, 9, 12, 30, 0, TimeSpan.FromHours(-5));
        var duration = TimeSpan.FromMilliseconds(245);
        var request = new RecordCheckResultRequest(
            endpoint.Id,
            checkedAt,
            duration,
            applicationOutcome,
            HttpStatusCode: applicationOutcome == ApplicationCheckOutcome.Succeeded ? 200 : 503,
            FailureDetail: applicationOutcome == ApplicationCheckOutcome.Failed ? "Service unavailable" : null);

        var summary = await useCase.ExecuteAsync(request);

        var persistedResult = Assert.Single(checkResultRepository.AddedResults);
        Assert.Equal(expectedDomainOutcome, persistedResult.Outcome);
        Assert.Equal(endpoint.Id, persistedResult.EndpointId);
        Assert.Equal(checkedAt.ToUniversalTime(), persistedResult.CheckedAt);
        Assert.Equal(duration, persistedResult.Duration);
        Assert.Equal(request.HttpStatusCode, persistedResult.HttpStatusCode);
        Assert.Equal(request.FailureDetail, persistedResult.FailureDetail);
        Assert.Equal(persistedResult.Id, summary.Id);
        Assert.Equal(persistedResult.EndpointId, summary.EndpointId);
        Assert.Equal(persistedResult.CheckedAt, summary.CheckedAt);
        Assert.Equal(persistedResult.Duration, summary.Duration);
        Assert.Equal(applicationOutcome, summary.Outcome);
        Assert.Equal(persistedResult.HttpStatusCode, summary.HttpStatusCode);
        Assert.Equal(persistedResult.FailureDetail, summary.FailureDetail);
        Assert.Equal(checkedAt.ToUniversalTime(), endpoint.LastCheckedAt);
        Assert.Same(endpoint, Assert.Single(endpointRepository.UpdatedEndpoints));
    }

    [Fact]
    public async Task ExecuteAsync_EndpointDoesNotExist_ThrowsWithoutWriting()
    {
        var endpointRepository = new InMemoryEndpointRepository();
        var checkResultRepository = new InMemoryCheckResultRepository();
        var useCase = new RecordCheckResult(endpointRepository, checkResultRepository);
        var endpointId = Guid.NewGuid();
        var request = new RecordCheckResultRequest(
            endpointId,
            DateTimeOffset.UtcNow,
            TimeSpan.FromMilliseconds(100),
            ApplicationCheckOutcome.Succeeded);

        var exception = await Assert.ThrowsAsync<EndpointNotFoundException>(() => useCase.ExecuteAsync(request));

        Assert.Equal(endpointId, exception.EndpointId);
        Assert.Empty(endpointRepository.AddedEndpoints);
        Assert.Empty(endpointRepository.UpdatedEndpoints);
        Assert.Empty(checkResultRepository.AddedResults);
    }
}