namespace UptimeMonitor.Application.DTOs;

public sealed record RecordCheckResultRequest(
    Guid EndpointId,
    DateTimeOffset CheckedAt,
    TimeSpan Duration,
    CheckOutcome Outcome,
    int? HttpStatusCode = null,
    string? FailureDetail = null);