using UptimeMonitor.Domain;

namespace UptimeMonitor.Application.DTOs;

public sealed record EndpointSummary(
    Guid Id,
    string Name,
    string Url,
    TimeSpan CheckInterval,
    DateTimeOffset? LastCheckedAt,
    bool IsEnabled)
{
    public static EndpointSummary From(MonitoredEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        return new EndpointSummary(
            endpoint.Id,
            endpoint.Name,
            endpoint.Url.ToString(),
            endpoint.CheckInterval,
            endpoint.LastCheckedAt,
            endpoint.IsEnabled);
    }
}