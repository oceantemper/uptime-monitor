using UptimeMonitor.Infrastructure.Persistence;

namespace UptimeMonitor.Infrastructure.Tests;

[Trait("Category", "Integration")]
public sealed class EndpointRepositoryIntegrationTests(PostgresDatabaseFixture fixture)
    : IClassFixture<PostgresDatabaseFixture>
{
    [Fact]
    public async Task AddAndGetByIdAsync_EndpointHasUriAndInterval_RoundTripsFromPostgres()
    {
        await using var data = new PostgresTestData(fixture.Options);
        var endpoint = data.CreateEndpoint(
            "Round trip endpoint " + Guid.NewGuid(),
            "https://example.test/health?region=west",
            TimeSpan.FromMinutes(7) + TimeSpan.FromSeconds(13));

        await using (var dbContext = data.CreateDbContext())
        {
            var repository = new EndpointRepository(dbContext);
            var unitOfWork = new EfUnitOfWork(dbContext);
            await repository.AddAsync(endpoint);
            await unitOfWork.SaveChangesAsync();
        }

        await using var verificationContext = data.CreateDbContext();
        var persisted = await new EndpointRepository(verificationContext).GetByIdAsync(endpoint.Id);

        Assert.NotNull(persisted);
        Assert.Equal(endpoint.Url, persisted.Url);
        Assert.Equal(endpoint.CheckInterval, persisted.CheckInterval);
        Assert.Contains(await new EndpointRepository(verificationContext).GetAllAsync(), item => item.Id == endpoint.Id);
    }

    [Fact]
    public async Task UpdateAsync_EnabledAndLastCheckedStateChanges_PersistAcrossContexts()
    {
        await using var data = new PostgresTestData(fixture.Options);
        var endpoint = data.CreateEndpoint(
            "Mutable endpoint " + Guid.NewGuid(),
            "https://example.test/mutable",
            TimeSpan.FromMinutes(3));

        await using (var dbContext = data.CreateDbContext())
        {
            var repository = new EndpointRepository(dbContext);
            await repository.AddAsync(endpoint);
            await new EfUnitOfWork(dbContext).SaveChangesAsync();

            var checkedAt = new DateTimeOffset(2031, 4, 5, 6, 7, 8, TimeSpan.Zero);
            endpoint.Disable();
            endpoint.MarkChecked(checkedAt);
            await repository.UpdateAsync(endpoint);
            await new EfUnitOfWork(dbContext).SaveChangesAsync();
        }

        await using (var verificationContext = data.CreateDbContext())
        {
            var persisted = await new EndpointRepository(verificationContext).GetByIdAsync(endpoint.Id);

            Assert.NotNull(persisted);
            Assert.False(persisted.IsEnabled);
            Assert.Equal(new DateTimeOffset(2031, 4, 5, 6, 7, 8, TimeSpan.Zero), persisted.LastCheckedAt);
            persisted.Enable();
            await new EndpointRepository(verificationContext).UpdateAsync(persisted);
            await new EfUnitOfWork(verificationContext).SaveChangesAsync();
        }

        await using var enabledContext = data.CreateDbContext();
        var enabled = await new EndpointRepository(enabledContext).GetByIdAsync(endpoint.Id);
        Assert.NotNull(enabled);
        Assert.True(enabled.IsEnabled);
    }

    [Fact]
    public async Task GetDueForCheckingAsync_EndpointsAreFilteredByIsDue()
    {
        await using var data = new PostgresTestData(fixture.Options);
        var now = new DateTimeOffset(2032, 8, 9, 10, 11, 12, TimeSpan.Zero);
        var interval = TimeSpan.FromMinutes(5);
        var neverChecked = data.CreateEndpoint("Never checked " + Guid.NewGuid(), "https://example.test/a", interval);
        var due = data.CreateEndpoint("Due " + Guid.NewGuid(), "https://example.test/b", interval);
        due.MarkChecked(now - TimeSpan.FromMinutes(6));
        var notDue = data.CreateEndpoint("Not due " + Guid.NewGuid(), "https://example.test/c", interval);
        notDue.MarkChecked(now - TimeSpan.FromMinutes(4));
        var disabled = data.CreateEndpoint("Disabled " + Guid.NewGuid(), "https://example.test/d", interval);
        disabled.MarkChecked(now - TimeSpan.FromMinutes(20));
        disabled.Disable();

        await using (var dbContext = data.CreateDbContext())
        {
            var repository = new EndpointRepository(dbContext);
            foreach (var endpoint in new[] { neverChecked, due, notDue, disabled })
            {
                await repository.AddAsync(endpoint);
            }

            await new EfUnitOfWork(dbContext).SaveChangesAsync();
        }

        await using var verificationContext = data.CreateDbContext();
        var dueEndpoints = await new EndpointRepository(verificationContext).GetDueForCheckingAsync(now);
        var ownedEndpointIds = new[] { neverChecked.Id, due.Id, notDue.Id, disabled.Id };

        Assert.Equal(
            new[] { neverChecked.Id, due.Id }.OrderBy(id => id),
            dueEndpoints
                .Where(endpoint => ownedEndpointIds.Contains(endpoint.Id))
                .Select(endpoint => endpoint.Id)
                .OrderBy(id => id));
    }
}