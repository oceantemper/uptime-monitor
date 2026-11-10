namespace UptimeMonitor.Domain;

public sealed class MonitoredEndpoint
{
    private MonitoredEndpoint()
    {
        Name = null!;
        Url = null!;
    }

    public MonitoredEndpoint(string name, string url, TimeSpan checkInterval)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var parsedUrl) ||
            (parsedUrl.Scheme != Uri.UriSchemeHttp && parsedUrl.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("URL must be an absolute HTTP or HTTPS URL.", nameof(url));
        }

        if (checkInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(checkInterval), "Check interval must be positive.");
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
        Url = parsedUrl;
        CheckInterval = checkInterval;
        IsEnabled = true;
    }

    public Guid Id { get; }

    public string Name { get; }

    public Uri Url { get; }

    public TimeSpan CheckInterval { get; }

    public DateTimeOffset? LastCheckedAt { get; private set; }

    public bool IsEnabled { get; private set; }

    public void MarkChecked(DateTimeOffset at)
    {
        LastCheckedAt = at.ToUniversalTime();
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public bool IsDue(DateTimeOffset now)
    {
        return IsEnabled &&
            (LastCheckedAt is null || now - LastCheckedAt.Value >= CheckInterval);
    }
}