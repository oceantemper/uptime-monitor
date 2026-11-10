using UptimeMonitor.Application.Abstractions.Persistence;
using UptimeMonitor.Application.DTOs;
using UptimeMonitor.Application.Exceptions;
using UptimeMonitor.Domain;

namespace UptimeMonitor.Application.UseCases;

public sealed class RecordCheckResult(
    IEndpointRepository endpointRepository,
    ICheckResultRepository checkResultRepository)
{
    public async Task<CheckResultSummary> ExecuteAsync(
        RecordCheckResultRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var endpoint = await endpointRepository.GetByIdAsync(request.EndpointId, cancellationToken)
            ?? throw new EndpointNotFoundException(request.EndpointId);

        var result = new Domain.CheckResult(
            request.EndpointId,
            request.CheckedAt,
            request.Duration,
            ToDomainOutcome(request.Outcome),
            request.HttpStatusCode,
            request.FailureDetail);

        endpoint.MarkChecked(request.CheckedAt);
        await endpointRepository.UpdateAsync(endpoint, cancellationToken);
        await checkResultRepository.AddAsync(result, cancellationToken);

        return new CheckResultSummary(
            result.Id,
            result.EndpointId,
            result.CheckedAt,
            result.Duration,
            ToApplicationOutcome(result.Outcome),
            result.HttpStatusCode,
            result.FailureDetail);
    }

    private static Domain.CheckOutcome ToDomainOutcome(DTOs.CheckOutcome outcome)
    {
        return outcome switch
        {
            DTOs.CheckOutcome.Succeeded => Domain.CheckOutcome.Succeeded,
            DTOs.CheckOutcome.Failed => Domain.CheckOutcome.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Outcome is not defined.")
        };
    }

    private static DTOs.CheckOutcome ToApplicationOutcome(Domain.CheckOutcome outcome)
    {
        return outcome switch
        {
            Domain.CheckOutcome.Succeeded => DTOs.CheckOutcome.Succeeded,
            Domain.CheckOutcome.Failed => DTOs.CheckOutcome.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Outcome is not defined.")
        };
    }
}