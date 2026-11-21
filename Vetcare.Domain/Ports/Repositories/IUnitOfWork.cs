namespace Vetcare.Domain.Ports.Repositories;

/// <summary>Commits all repository changes as one database operation.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
