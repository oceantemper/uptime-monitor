namespace UptimeMonitor.Application.DTOs;

public sealed record RegisterEndpointRequest(string Name, string Url, TimeSpan CheckInterval);