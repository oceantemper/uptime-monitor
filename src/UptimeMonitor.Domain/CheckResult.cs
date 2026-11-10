namespace UptimeMonitor.Domain;

public sealed class CheckResult
{
    public CheckResult(
        Guid endpointId,
        DateTimeOffset checkedAt,
        TimeSpan duration,
        CheckOutcome outcome,
        int? httpStatusCode = null,
        string? failureDetail = null)
    {
        if (endpointId == Guid.Empty)
        {
            throw new ArgumentException("Endpoint ID cannot be empty.", nameof(endpointId));
        }

        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration cannot be negative.");
        }

        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), "Outcome is not defined.");
        }

        if (httpStatusCode is < 100 or > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(httpStatusCode), "HTTP status code must be between 100 and 599.");
        }

        Id = Guid.NewGuid();
        EndpointId = endpointId;
        CheckedAt = checkedAt.ToUniversalTime();
        Duration = duration;
        Outcome = outcome;
        HttpStatusCode = httpStatusCode;
        FailureDetail = string.IsNullOrWhiteSpace(failureDetail) ? null : failureDetail.Trim();
    }

    public Guid Id { get; }

    public Guid EndpointId { get; }

    public DateTimeOffset CheckedAt { get; }

    public TimeSpan Duration { get; }

    public CheckOutcome Outcome { get; }

    public int? HttpStatusCode { get; }

    public string? FailureDetail { get; }
}