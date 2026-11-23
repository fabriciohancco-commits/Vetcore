using Microsoft.EntityFrameworkCore;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Infrastructure.Adapters.Repositories.Implementation;

public sealed class OperacionRepository(VetCareDbContext db) : IOperacionRepository
{
    public Task<List<Operacion>> ListAsync(CancellationToken cancellationToken = default) =>
        db.Operaciones.AsNoTracking().Include(o => o.Mascota)
            .OrderBy(o => o.FechaProgramada).ToListAsync(cancellationToken);

    public Task<Operacion?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Operaciones.Include(o => o.Mascota).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public void Add(Operacion operacion) => db.Operaciones.Add(operacion);

    public void Remove(Operacion operacion) => db.Operaciones.Remove(operacion);
}
