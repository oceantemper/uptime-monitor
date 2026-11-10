using UptimeMonitor.Domain;

namespace UptimeMonitor.Domain.Tests;

public class CheckResultTests
{
    [Fact]
    public void Constructor_ValidValues_AssignsValuesAndNormalizesCheckedAtToUtc()
    {
        var endpointId = Guid.NewGuid();
        var checkedAt = new DateTimeOffset(2026, 11, 9, 12, 30, 0, TimeSpan.FromHours(5));
        var duration = TimeSpan.FromMilliseconds(125);

        var result = new CheckResult(
            endpointId,
            checkedAt,
            duration,
            CheckOutcome.Failed,
            httpStatusCode: 503,
            failureDetail: "  Service unavailable  ");

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(endpointId, result.EndpointId);
        Assert.Equal(checkedAt.ToUniversalTime(), result.CheckedAt);
        Assert.Equal(TimeSpan.Zero, result.CheckedAt.Offset);
        Assert.Equal(duration, result.Duration);
        Assert.Equal(CheckOutcome.Failed, result.Outcome);
        Assert.Equal(503, result.HttpStatusCode);
        Assert.Equal("Service unavailable", result.FailureDetail);
    }

    [Fact]
    public void Constructor_OptionalStatusAndFailureDetailOmitted_AssignsNulls()
    {
        var result = new CheckResult(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            TimeSpan.Zero,
            CheckOutcome.Succeeded);

        Assert.Null(result.HttpStatusCode);
        Assert.Null(result.FailureDetail);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public void Constructor_EmptyOrWhitespaceFailureDetail_AssignsNull(string? failureDetail)
    {
        var result = new CheckResult(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            TimeSpan.Zero,
            CheckOutcome.Failed,
            failureDetail: failureDetail);

        Assert.Null(result.FailureDetail);
    }

    [Fact]
    public void Constructor_EmptyEndpointId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new CheckResult(Guid.Empty, DateTimeOffset.UtcNow, TimeSpan.Zero, CheckOutcome.Succeeded));

        Assert.Equal("endpointId", exception.ParamName);
    }

    [Fact]
    public void Constructor_NegativeDuration_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new CheckResult(Guid.NewGuid(), DateTimeOffset.UtcNow, TimeSpan.FromTicks(-1), CheckOutcome.Failed));

        Assert.Equal("duration", exception.ParamName);
    }

    [Fact]
    public void Constructor_UndefinedOutcome_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new CheckResult(Guid.NewGuid(), DateTimeOffset.UtcNow, TimeSpan.Zero, (CheckOutcome)int.MaxValue));

        Assert.Equal("outcome", exception.ParamName);
    }

    [Theory]
    [InlineData(99)]
    [InlineData(600)]
    public void Constructor_StatusCodeOutsideHttpRange_ThrowsArgumentOutOfRangeException(int statusCode)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new CheckResult(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                TimeSpan.Zero,
                CheckOutcome.Failed,
                httpStatusCode: statusCode));

        Assert.Equal("httpStatusCode", exception.ParamName);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(599)]
    public void Constructor_StatusCodeAtHttpRangeBoundary_AssignsStatusCode(int statusCode)
    {
        var result = new CheckResult(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            TimeSpan.Zero,
            CheckOutcome.Succeeded,
            httpStatusCode: statusCode);

        Assert.Equal(statusCode, result.HttpStatusCode);
    }
}