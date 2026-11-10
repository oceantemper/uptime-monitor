namespace UptimeMonitor.Application.Exceptions;

public sealed class EndpointNotFoundException(Guid endpointId)
    : Exception($"Monitored endpoint with ID '{endpointId}' was not found.")
{
    public Guid EndpointId { get; } = endpointId;
}