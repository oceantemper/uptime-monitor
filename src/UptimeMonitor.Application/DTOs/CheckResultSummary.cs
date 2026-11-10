namespace UptimeMonitor.Application.DTOs;

public sealed record CheckResultSummary(
    Guid Id,
    Guid EndpointId,
    DateTimeOffset CheckedAt,
    TimeSpan Duration,
    CheckOutcome Outcome,
    int? HttpStatusCode,
    string? FailureDetail)
{
    public static CheckResultSummary From(Domain.CheckResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var outcome = result.Outcome switch
        {
            Domain.CheckOutcome.Succeeded => CheckOutcome.Succeeded,
            Domain.CheckOutcome.Failed => CheckOutcome.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Outcome, "Outcome is not defined.")
        };

        return new CheckResultSummary(
            result.Id,
            result.EndpointId,
            result.CheckedAt,
            result.Duration,
            outcome,
            result.HttpStatusCode,
            result.FailureDetail);
    }
}