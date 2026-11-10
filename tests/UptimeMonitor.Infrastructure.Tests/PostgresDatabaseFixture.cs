using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UptimeMonitor.Infrastructure.Persistence;

namespace UptimeMonitor.Infrastructure.Tests;

public sealed class PostgresDatabaseFixture : IAsyncLifetime
{
    public DbContextOptions<UptimeMonitorDbContext> Options { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var webAssembly = Assembly.Load("UptimeMonitor.Web");
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(webAssembly, optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "PostgreSQL integration tests require ConnectionStrings:Default in Web user-secrets " +
                "or ConnectionStrings__Default in the environment. The connection value is not displayed.");
        }

        Options = new DbContextOptionsBuilder<UptimeMonitorDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        try
        {
            await using var dbContext = new UptimeMonitorDbContext(Options);
            await dbContext.Database.MigrateAsync();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "Could not apply the existing EF Core migrations to the configured PostgreSQL database " +
                $"({exception.GetType().Name}). Ensure the real PostgreSQL service is reachable and the " +
                "configured database permits migrations. No database was dropped or recreated.");
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;
}