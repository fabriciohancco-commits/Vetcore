using Microsoft.EntityFrameworkCore;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Infrastructure.Adapters.Repositories.Implementation;

public sealed class MascotaRepository(VetCareDbContext db) : IMascotaRepository
{
    public Task<List<Mascota>> ListAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query = db.Mascotas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(pet => pet.Nombre.Contains(term)
                || pet.Especie.Contains(term)
                || (pet.Raza != null && pet.Raza.Contains(term)));
        }

        return query.OrderBy(pet => pet.Nombre).ToListAsync(cancellationToken);
    }

    public ValueTask<Mascota?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Mascotas.FindAsync([id], cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        db.Mascotas.AnyAsync(pet => pet.Id == id, cancellationToken);

    public void Add(Mascota mascota) => db.Mascotas.Add(mascota);

    public void Remove(Mascota mascota) => db.Mascotas.Remove(mascota);
}
