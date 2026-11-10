using UptimeMonitor.Application.Abstractions.Persistence;
using UptimeMonitor.Application.DTOs;
using UptimeMonitor.Domain;

namespace UptimeMonitor.Application.UseCases;

public sealed class RegisterEndpoint(IEndpointRepository endpointRepository)
{
    public async Task<EndpointSummary> ExecuteAsync(
        RegisterEndpointRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var endpoint = new MonitoredEndpoint(request.Name, request.Url, request.CheckInterval);
        await endpointRepository.AddAsync(endpoint, cancellationToken);

        return EndpointSummary.From(endpoint);
    }
}