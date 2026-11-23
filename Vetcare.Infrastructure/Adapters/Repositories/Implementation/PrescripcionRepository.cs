using Microsoft.EntityFrameworkCore;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Infrastructure.Adapters.Repositories.Implementation;

public sealed class PrescripcionRepository(VetCareDbContext db) : IPrescripcionRepository
{
    public Task<List<Prescripcion>> ListAsync(
        int? mascotaId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.Prescripciones.AsNoTracking()
            .Include(p => p.Mascota)
            .Include(p => p.Medicamento)
            .AsQueryable();
        if (mascotaId.HasValue) query = query.Where(p => p.MascotaId == mascotaId.Value);
        return query.OrderByDescending(p => p.FechaRegistro).ToListAsync(cancellationToken);
    }

    public Task<Prescripcion?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Prescripciones.Include(p => p.Mascota).Include(p => p.Medicamento)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Add(Prescripcion prescripcion) => db.Prescripciones.Add(prescripcion);
}
