using UptimeMonitor.Domain;
using UptimeMonitor.Infrastructure.Persistence;

namespace UptimeMonitor.Infrastructure.Tests;

[Trait("Category", "Integration")]
public sealed class CheckResultRepositoryIntegrationTests(PostgresDatabaseFixture fixture)
    : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task AddAndGetForEndpointSinceAsync_ResultsAreFilteredOrderedAndRoundTrip()
    {
        await using var data = new PostgresTestData(fixture.Options);
        var endpoint = data.CreateEndpoint(
            "Result endpoint " + Guid.NewGuid(),
            "https://example.test/results",
            TimeSpan.FromMinutes(2));
        var otherEndpoint = data.CreateEndpoint(
            "Other result endpoint " + Guid.NewGuid(),
            "https://example.test/other-results",
            TimeSpan.FromMinutes(2));
        var since = new DateTimeOffset(2033, 2, 3, 4, 5, 6, TimeSpan.Zero);
        var earlier = new CheckResult(endpoint.Id, since - TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(125), CheckOutcome.Succeeded, 200);
        var later = new CheckResult(endpoint.Id, since + TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(875), CheckOutcome.Failed, 503, "  upstream unavailable  ");
        var atBoundary = new CheckResult(endpoint.Id, since, TimeSpan.FromMilliseconds(250), CheckOutcome.Succeeded, 204);
        var unrelated = new CheckResult(otherEndpoint.Id, since + TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), CheckOutcome.Failed, 502);

        await using (var dbContext = data.CreateDbContext())
        {
            var endpointRepository = new EndpointRepository(dbContext);
            await endpointRepository.AddAsync(endpoint);
            await endpointRepository.AddAsync(otherEndpoint);

            var repository = new CheckResultRepository(dbContext);
            foreach (var result in new[] { later, unrelated, earlier, atBoundary })
            {
                await repository.AddAsync(result);
            }

            await new EfUnitOfWork(dbContext).SaveChangesAsync();
        }

        await using var verificationContext = data.CreateDbContext();
        var persistedResults = await new CheckResultRepository(verificationContext)
            .GetForEndpointSinceAsync(endpoint.Id, since);

        Assert.Equal(new[] { atBoundary.Id, later.Id }, persistedResults.Select(result => result.Id));
        Assert.Equal(TimeSpan.FromMilliseconds(250), persistedResults[0].Duration);
        Assert.Equal(TimeSpan.FromMilliseconds(875), persistedResults[1].Duration);
        Assert.Equal(CheckOutcome.Failed, persistedResults[1].Outcome);
        Assert.Equal(503, persistedResults[1].HttpStatusCode);
        Assert.Equal("upstream unavailable", persistedResults[1].FailureDetail);
    }
}