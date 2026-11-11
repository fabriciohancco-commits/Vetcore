using Vetcare.Domain.Entities;

namespace Vetcare.Application.Abstractions.Persistence;

public interface IMascotaRepository
{
    Task<List<Mascota>> ListAsync(CancellationToken cancellationToken);
    Task<Mascota?> FindByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    void Add(Mascota mascota);
    void Remove(Mascota mascota);
}
