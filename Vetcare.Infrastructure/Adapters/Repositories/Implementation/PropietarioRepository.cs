using Microsoft.EntityFrameworkCore;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Infrastructure.Adapters.Repositories.Implementation;

public sealed class PropietarioRepository(VetCareDbContext db) : IPropietarioRepository
{
    public Task<List<Propietario>> ListAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query = db.Propietarios.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(owner => owner.Nombres.Contains(term)
                || owner.Apellidos.Contains(term)
                || owner.Telefono.Contains(term)
                || (owner.Correo != null && owner.Correo.Contains(term)));
        }

        return query.OrderBy(owner => owner.Apellidos)
            .ThenBy(owner => owner.Nombres)
            .ToListAsync(cancellationToken);
    }

    public ValueTask<Propietario?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Propietarios.FindAsync([id], cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        db.Propietarios.AnyAsync(owner => owner.Id == id, cancellationToken);

    public Task<bool> HasMascotasAsync(int id, CancellationToken cancellationToken = default) =>
        db.Mascotas.AnyAsync(pet => pet.PropietarioId == id, cancellationToken);

    public void Add(Propietario propietario) => db.Propietarios.Add(propietario);

    public void Remove(Propietario propietario) => db.Propietarios.Remove(propietario);
}
