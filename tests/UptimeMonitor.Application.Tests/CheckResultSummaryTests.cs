using UptimeMonitor.Application.DTOs;
using UptimeMonitor.Domain;
using ApplicationCheckOutcome = UptimeMonitor.Application.DTOs.CheckOutcome;
using DomainCheckOutcome = UptimeMonitor.Domain.CheckOutcome;

namespace UptimeMonitor.Application.Tests;

public sealed class CheckResultSummaryTests
{
    [Theory]
    [InlineData(DomainCheckOutcome.Succeeded, ApplicationCheckOutcome.Succeeded, 200, null)]
    [InlineData(DomainCheckOutcome.Failed, ApplicationCheckOutcome.Failed, 503, "Service unavailable")]
    public void From_DomainCheckResult_CopiesFieldsAndMapsOutcome(
        DomainCheckOutcome domainOutcome,
        ApplicationCheckOutcome expectedOutcome,
        int httpStatusCode,
        string? failureDetail)
    {
        var endpointId = Guid.NewGuid();
        var checkedAt = new DateTimeOffset(2026, 11, 9, 12, 30, 0, TimeSpan.FromHours(-5));
        var duration = TimeSpan.FromMilliseconds(245);
        var result = new CheckResult(
            endpointId,
            checkedAt,
            duration,
            domainOutcome,
            httpStatusCode,
            failureDetail);

        var summary = CheckResultSummary.From(result);

        Assert.Equal(result.Id, summary.Id);
        Assert.Equal(result.EndpointId, summary.EndpointId);
        Assert.Equal(result.CheckedAt, summary.CheckedAt);
        Assert.Equal(result.Duration, summary.Duration);
        Assert.Equal(expectedOutcome, summary.Outcome);
        Assert.Equal(result.HttpStatusCode, summary.HttpStatusCode);
        Assert.Equal(result.FailureDetail, summary.FailureDetail);
    }
}