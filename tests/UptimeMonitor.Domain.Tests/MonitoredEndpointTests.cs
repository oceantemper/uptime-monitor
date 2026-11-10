using UptimeMonitor.Domain;

namespace UptimeMonitor.Domain.Tests;

public class MonitoredEndpointTests
{
    [Fact]
    public void Constructor_ValidValues_AssignsTrimmedNameUrlAndInterval()
    {
        var interval = TimeSpan.FromMinutes(5);

        var endpoint = new MonitoredEndpoint("  Status page  ", " https://example.com/status ", interval);

        Assert.NotEqual(Guid.Empty, endpoint.Id);
        Assert.Equal("Status page", endpoint.Name);
        Assert.Equal(new Uri("https://example.com/status"), endpoint.Url);
        Assert.True(endpoint.Url.IsAbsoluteUri);
        Assert.Equal(interval, endpoint.CheckInterval);
    }

    [Fact]
    public void IsDue_NeverCheckedAndEnabled_ReturnsTrue()
    {
        var endpoint = new MonitoredEndpoint("Status page", "https://example.com", TimeSpan.FromMinutes(5));

        Assert.True(endpoint.IsEnabled);
        Assert.Null(endpoint.LastCheckedAt);
        Assert.True(endpoint.IsDue(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void IsDue_AtCheckIntervalBoundaryAfterMarkChecked_ReturnsTrue()
    {
        var interval = TimeSpan.FromMinutes(5);
        var endpoint = new MonitoredEndpoint("Status page", "https://example.com", interval);
        var checkedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        endpoint.MarkChecked(checkedAt);

        Assert.True(endpoint.IsDue(checkedAt + interval));
    }

    [Fact]
    public void IsDue_DisabledAndNeverChecked_ReturnsFalse()
    {
        var endpoint = new MonitoredEndpoint("Status page", "https://example.com", TimeSpan.FromMinutes(5));
        endpoint.Disable();

        Assert.False(endpoint.IsEnabled);
        Assert.Null(endpoint.LastCheckedAt);
        Assert.False(endpoint.IsDue(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Enable_DisabledEndpoint_SetsEnabled()
    {
        var endpoint = new MonitoredEndpoint("Status page", "https://example.com", TimeSpan.FromMinutes(5));
        endpoint.Disable();

        endpoint.Enable();

        Assert.True(endpoint.IsEnabled);
    }

    [Fact]
    public void MarkChecked_AtOffsetTime_StoresUtcTimestamp()
    {
        var endpoint = new MonitoredEndpoint("Status page", "https://example.com", TimeSpan.FromMinutes(5));
        var checkedAt = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.FromHours(3));

        endpoint.MarkChecked(checkedAt);

        Assert.Equal(checkedAt.ToUniversalTime(), endpoint.LastCheckedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public void Constructor_InvalidName_ThrowsArgumentException(string? name)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new MonitoredEndpoint(name!, "https://example.com", TimeSpan.FromMinutes(1)));

        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("status/health")]
    [InlineData("ftp://example.com/status")]
    [InlineData("mailto:ops@example.com")]
    public void Constructor_InvalidUrl_ThrowsArgumentException(string? url)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new MonitoredEndpoint("Status page", url!, TimeSpan.FromMinutes(1)));

        Assert.Equal("url", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NonPositiveCheckInterval_ThrowsArgumentOutOfRangeException(int minutes)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new MonitoredEndpoint("Status page", "https://example.com", TimeSpan.FromMinutes(minutes)));

        Assert.Equal("checkInterval", exception.ParamName);
    }
}