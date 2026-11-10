using UptimeMonitor.Application.DTOs;
using UptimeMonitor.Domain;

namespace UptimeMonitor.Application.Tests;

public sealed class EndpointSummaryTests
{
    [Fact]
    public void From_EndpointWithState_CopiesFieldsAndFormatsUrlAsString()
    {
        var endpoint = new MonitoredEndpoint("Internal API", "https://example.com:8443/status", TimeSpan.FromSeconds(45));
        var lastCheckedAt = new DateTimeOffset(2026, 11, 9, 16, 0, 0, TimeSpan.Zero);
        endpoint.MarkChecked(lastCheckedAt);
        endpoint.Disable();

        var summary = EndpointSummary.From(endpoint);

        Assert.Equal(endpoint.Id, summary.Id);
        Assert.Equal("Internal API", summary.Name);
        Assert.Equal("https://example.com:8443/status", summary.Url);
        Assert.Equal(TimeSpan.FromSeconds(45), summary.CheckInterval);
        Assert.Equal(lastCheckedAt, summary.LastCheckedAt);
        Assert.False(summary.IsEnabled);
    }
}