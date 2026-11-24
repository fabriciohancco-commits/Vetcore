using Microsoft.EntityFrameworkCore;
using Vetcare.Application.Abstractions.Persistence;
using Vetcare.Domain.Entities;

namespace Vetcare.Infrastructure.Persistence.Repositories;

public sealed class MascotaRepository(VetCareDbContext db) : IMascotaRepository
{
    public Task<List<Mascota>> ListAsync(string? search, CancellationToken ct)
    {
        var query = db.Mascotas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m => m.Nombre.Contains(term) || m.Especie.Contains(term) ||
                (m.Raza != null && m.Raza.Contains(term)));
        }
        return query.OrderBy(m => m.Nombre).ToListAsync(ct);
    }
    public Task<Mascota?> FindByIdAsync(int id, CancellationToken ct) => db.Mascotas.FindAsync([id], ct).AsTask();
    public Task<bool> ExistsAsync(int id, CancellationToken ct) => db.Mascotas.AnyAsync(m => m.Id == id, ct);
    public Task<bool> HasConsultasAsync(int id, CancellationToken ct) => db.ConsultasMedicas.AnyAsync(c => c.MascotaId == id, ct);
    public void Add(Mascota mascota) => db.Mascotas.Add(mascota);
    public void Remove(Mascota mascota) => db.Mascotas.Remove(mascota);
}
