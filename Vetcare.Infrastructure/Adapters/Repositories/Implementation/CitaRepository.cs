using Microsoft.EntityFrameworkCore;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Infrastructure.Adapters.Repositories.Implementation;

public sealed class CitaRepository(VetCareDbContext db) : ICitaRepository
{
    public Task<List<Cita>> ListAsync(CancellationToken cancellationToken = default) =>
        db.Citas.AsNoTracking()
            .Include(c => c.Mascota)
            .Include(c => c.Propietario)
            .OrderBy(c => c.FechaHora)
            .ToListAsync(cancellationToken);

    public Task<List<Cita>> ListByVeterinarioAndPeriodAsync(
        string veterinario, DateTime desde, DateTime hasta,
        CancellationToken cancellationToken = default) =>
        db.Citas.AsNoTracking()
            .Where(c => c.Estado != "Cancelada"
                && c.FechaHora >= desde
                && c.FechaHora < hasta
                && c.VeterinarioNombre.ToLower() == veterinario.ToLower())
            .ToListAsync(cancellationToken);

    public Task<Cita?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Citas.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public void Add(Cita cita) => db.Citas.Add(cita);
}
