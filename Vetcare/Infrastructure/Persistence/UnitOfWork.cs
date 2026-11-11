using Microsoft.EntityFrameworkCore;
using Vetcare.Application.Abstractions.Persistence;

namespace Vetcare.Infrastructure.Persistence;

public sealed class UnitOfWork(VetCareDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var result = await operation(ct);
        await transaction.CommitAsync(ct);
        return result;
    }
}
