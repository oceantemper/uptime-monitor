namespace UptimeMonitor.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    /// <summary>
    /// Persists all changes staged by repositories in the current unit of work atomically.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}