using Microsoft.EntityFrameworkCore;
using Vetcare.Application.Abstractions.Persistence;
using Vetcare.Domain.Entities;

namespace Vetcare.Infrastructure.Persistence.Repositories;

public sealed class PropietarioRepository(VetCareDbContext db) : IPropietarioRepository
{
    public Task<List<Propietario>> ListAsync(CancellationToken ct) =>
        db.Propietarios.AsNoTracking().OrderBy(p => p.Apellidos).ThenBy(p => p.Nombres).ToListAsync(ct);
    public Task<Propietario?> FindByIdAsync(int id, CancellationToken ct) => db.Propietarios.FindAsync([id], ct).AsTask();
    public Task<bool> ExistsAsync(int id, CancellationToken ct) => db.Propietarios.AnyAsync(p => p.Id == id, ct);
    public Task<bool> HasMascotasAsync(int id, CancellationToken ct) => db.Mascotas.AnyAsync(m => m.PropietarioId == id, ct);
    public void Add(Propietario propietario) => db.Propietarios.Add(propietario);
    public void Remove(Propietario propietario) => db.Propietarios.Remove(propietario);
}
