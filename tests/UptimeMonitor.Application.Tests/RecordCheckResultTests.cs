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
        var unitOfWork = new InMemoryUnitOfWork();
        var endpointRepository = new InMemoryEndpointRepository(unitOfWork);
        var checkResultRepository = new InMemoryCheckResultRepository(unitOfWork);
        var endpoint = new MonitoredEndpoint("Public API", "https://example.com/health", TimeSpan.FromMinutes(1));
        endpointRepository.Seed(endpoint);
        var useCase = new RecordCheckResult(endpointRepository, checkResultRepository, unitOfWork);
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

        var stagedResult = Assert.Single(checkResultRepository.StagedAddedResults);
        var persistedResult = Assert.Single(checkResultRepository.PersistedResults);
        var stagedUpdate = Assert.Single(endpointRepository.StagedUpdatedEndpoints);
        var persistedEndpoint = Assert.Single(endpointRepository.PersistedEndpoints);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
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
        Assert.Equal(persistedResult.Id, stagedResult.Id);
        Assert.Equal(endpoint.Id, stagedUpdate.Id);
        Assert.Equal(checkedAt.ToUniversalTime(), persistedEndpoint.LastCheckedAt);
    }

    [Fact]
    public async Task ExecuteAsync_EndpointDoesNotExist_ThrowsWithoutWriting()
    {
        var unitOfWork = new InMemoryUnitOfWork();
        var endpointRepository = new InMemoryEndpointRepository(unitOfWork);
        var checkResultRepository = new InMemoryCheckResultRepository(unitOfWork);
        var useCase = new RecordCheckResult(endpointRepository, checkResultRepository, unitOfWork);
        var endpointId = Guid.NewGuid();
        var request = new RecordCheckResultRequest(
            endpointId,
            DateTimeOffset.UtcNow,
            TimeSpan.FromMilliseconds(100),
            ApplicationCheckOutcome.Succeeded);

        var exception = await Assert.ThrowsAsync<EndpointNotFoundException>(() => useCase.ExecuteAsync(request));

        Assert.Equal(endpointId, exception.EndpointId);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        Assert.Empty(endpointRepository.StagedAddedEndpoints);
        Assert.Empty(endpointRepository.StagedUpdatedEndpoints);
        Assert.Empty(endpointRepository.PersistedAddedEndpoints);
        Assert.Empty(checkResultRepository.StagedAddedResults);
        Assert.Empty(checkResultRepository.PersistedResults);
    }
}