namespace UptimeMonitor.Application.DTOs;

public sealed record CheckResultSummary(
    Guid Id,
    Guid EndpointId,
    DateTimeOffset CheckedAt,
    TimeSpan Duration,
    CheckOutcome Outcome,
    int? HttpStatusCode,
    string? FailureDetail);