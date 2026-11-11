using Microsoft.EntityFrameworkCore;
using Vetcare.Application.Abstractions.Persistence;
using Vetcare.Domain.Entities;

namespace Vetcare.Infrastructure.Persistence.Repositories;

public sealed class MascotaRepository(VetCareDbContext db) : IMascotaRepository
{
    public Task<List<Mascota>> ListAsync(CancellationToken ct) =>
        db.Mascotas.AsNoTracking().OrderBy(m => m.Nombre).ToListAsync(ct);
    public Task<Mascota?> FindByIdAsync(int id, CancellationToken ct) => db.Mascotas.FindAsync([id], ct).AsTask();
    public Task<bool> ExistsAsync(int id, CancellationToken ct) => db.Mascotas.AnyAsync(m => m.Id == id, ct);
    public void Add(Mascota mascota) => db.Mascotas.Add(mascota);
    public void Remove(Mascota mascota) => db.Mascotas.Remove(mascota);
}
